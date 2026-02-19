using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleAccounts.Data;
using SimpleAccounts.Data.Models;
using SimpleAccounts.Data.Services;

namespace SimpleAccounts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuotesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITaxService _taxService;

    public QuotesController(ApplicationDbContext context, ITaxService taxService)
    {
        _context = context;
        _taxService = taxService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Quote>>> GetQuotes()
    {
        return await _context.Quotes
            .Include(q => q.Customer)
            .Include(q => q.QuoteLines)
                .ThenInclude(ql => ql.StockItem)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Quote>> GetQuote(int id)
    {
        var quote = await _context.Quotes
            .Include(q => q.Customer)
            .Include(q => q.QuoteLines)
                .ThenInclude(ql => ql.StockItem)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null)
        {
            return NotFound();
        }

        return quote;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,SalesUser")]
    public async Task<ActionResult<Quote>> PostQuote(Quote quote)
    {
        // Calculate totals
        quote.SubTotal = quote.QuoteLines.Sum(ql => ql.LineTotal);
        
        var customer = await _context.Customers.FindAsync(quote.CustomerId);
        if (customer != null)
        {
            quote.TaxAmount = _taxService.CalculateTax(quote.SubTotal, customer.Country);
        }
        
        quote.TotalAmount = quote.SubTotal + quote.TaxAmount;
        quote.CreatedDate = DateTime.UtcNow;
        
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, quote);
    }

    [HttpGet("{id}/pdf")]
    [Authorize(Roles = "Admin,Manager,SalesUser")]
    public async Task<IActionResult> GenerateQuotePdf(int id)
    {
        var quote = await _context.Quotes
            .Include(q => q.Customer)
            .Include(q => q.QuoteLines)
                .ThenInclude(ql => ql.StockItem)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null)
        {
            return NotFound();
        }

        // Simple PDF generation (placeholder - would need a PDF library like QuestPDF or iTextSharp)
        var pdfContent = $"QUOTE #{quote.QuoteNumber}\n\n" +
                        $"Date: {quote.QuoteDate:yyyy-MM-dd}\n" +
                        $"Customer: {quote.Customer.Name}\n\n" +
                        $"Items:\n";

        foreach (var line in quote.QuoteLines)
        {
            pdfContent += $"{line.StockItem.Name} - Qty: {line.Quantity} @ ${line.UnitPrice} = ${line.LineTotal}\n";
        }

        pdfContent += $"\nSubtotal: ${quote.SubTotal}\n" +
                     $"Tax: ${quote.TaxAmount}\n" +
                     $"Total: ${quote.TotalAmount}";

        var bytes = System.Text.Encoding.UTF8.GetBytes(pdfContent);
        return File(bytes, "text/plain", $"Quote_{quote.QuoteNumber}.txt");
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,SalesUser")]
    public async Task<IActionResult> PutQuote(int id, Quote quote)
    {
        if (id != quote.Id)
        {
            return BadRequest();
        }

        quote.LastModifiedDate = DateTime.UtcNow;
        _context.Entry(quote).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!QuoteExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteQuote(int id)
    {
        var quote = await _context.Quotes.FindAsync(id);
        if (quote == null)
        {
            return NotFound();
        }

        _context.Quotes.Remove(quote);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/convert-to-order")]
    [Authorize(Roles = "Admin,Manager,SalesUser")]
    public async Task<ActionResult<SalesOrder>> ConvertQuoteToSalesOrder(int id)
    {
        var quote = await _context.Quotes
            .Include(q => q.Customer)
            .Include(q => q.QuoteLines)
                .ThenInclude(ql => ql.StockItem)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null)
        {
            return NotFound();
        }

        // Create a new sales order from the quote with unique order number
        var salesOrder = new SalesOrder
        {
            CustomerId = quote.CustomerId,
            OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".Substring(0, 30),
            OrderDate = DateTime.UtcNow,
            Status = "Draft",
            SubTotal = quote.SubTotal,
            TaxAmount = quote.TaxAmount,
            TotalAmount = quote.TotalAmount,
            Notes = $"Converted from Quote #{quote.QuoteNumber}",
            CreatedDate = DateTime.UtcNow,
            OrderLines = new List<SalesOrderLine>()
        };

        // Convert quote lines to sales order lines
        foreach (var quoteLine in quote.QuoteLines)
        {
            salesOrder.OrderLines.Add(new SalesOrderLine
            {
                StockItemId = quoteLine.StockItemId,
                Quantity = quoteLine.Quantity,
                UnitPrice = quoteLine.UnitPrice,
                Discount = quoteLine.Discount,
                LineTotal = quoteLine.LineTotal
            });
        }

        _context.SalesOrders.Add(salesOrder);

        // Update quote status to "Accepted"
        quote.Status = "Accepted";
        quote.LastModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return CreatedAtAction("GetSalesOrder", "SalesOrders", new { id = salesOrder.Id }, salesOrder);
    }

    private bool QuoteExists(int id)
    {
        return _context.Quotes.Any(e => e.Id == id);
    }
}
