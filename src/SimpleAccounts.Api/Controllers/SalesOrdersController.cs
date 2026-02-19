using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleAccounts.Data;
using SimpleAccounts.Data.Models;
using SimpleAccounts.Data.Services;

namespace SimpleAccounts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesOrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITaxService _taxService;

    public SalesOrdersController(ApplicationDbContext context, ITaxService taxService)
    {
        _context = context;
        _taxService = taxService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesOrder>>> GetSalesOrders()
    {
        return await _context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.OrderLines)
                .ThenInclude(ol => ol.StockItem)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SalesOrder>> GetSalesOrder(int id)
    {
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.OrderLines)
                .ThenInclude(ol => ol.StockItem)
            .FirstOrDefaultAsync(so => so.Id == id);

        if (salesOrder == null)
        {
            return NotFound();
        }

        return salesOrder;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,SalesUser")]
    public async Task<ActionResult<SalesOrder>> PostSalesOrder(SalesOrder salesOrder)
    {
        // Calculate totals
        salesOrder.SubTotal = salesOrder.OrderLines.Sum(ol => ol.LineTotal);
        
        var customer = await _context.Customers.FindAsync(salesOrder.CustomerId);
        if (customer != null)
        {
            salesOrder.TaxAmount = _taxService.CalculateTax(salesOrder.SubTotal, customer.Country);
        }
        
        salesOrder.TotalAmount = salesOrder.SubTotal + salesOrder.TaxAmount;
        salesOrder.CreatedDate = DateTime.UtcNow;
        
        _context.SalesOrders.Add(salesOrder);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSalesOrder), new { id = salesOrder.Id }, salesOrder);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,SalesUser")]
    public async Task<IActionResult> PutSalesOrder(int id, SalesOrder salesOrder)
    {
        if (id != salesOrder.Id)
        {
            return BadRequest();
        }

        salesOrder.LastModifiedDate = DateTime.UtcNow;
        _context.Entry(salesOrder).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SalesOrderExists(id))
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
    public async Task<IActionResult> DeleteSalesOrder(int id)
    {
        var salesOrder = await _context.SalesOrders.FindAsync(id);
        if (salesOrder == null)
        {
            return NotFound();
        }

        _context.SalesOrders.Remove(salesOrder);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/generate-payment-link")]
    [Authorize(Roles = "Admin,Manager,SalesUser")]
    public async Task<ActionResult<PaymentLinkResponse>> GeneratePaymentLink(int id, [FromBody] PaymentLinkRequest request)
    {
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Customer)
            .FirstOrDefaultAsync(so => so.Id == id);

        if (salesOrder == null)
        {
            return NotFound();
        }

        // Generate a unique payment token
        var paymentToken = Guid.NewGuid().ToString("N");
        
        // In production, this would create an actual payment session with the payment provider
        var paymentLink = $"{Request.Scheme}://{Request.Host}/payment/{paymentToken}";
        
        return Ok(new PaymentLinkResponse
        {
            PaymentLink = paymentLink,
            PaymentToken = paymentToken,
            OrderId = salesOrder.Id,
            Amount = salesOrder.TotalAmount,
            Currency = request.Currency ?? "GBP",
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            PaymentMethod = request.PaymentMethod
        });
    }

    [HttpGet("{id}/pdf")]
    [Authorize(Roles = "Admin,Manager,SalesUser")]
    public async Task<IActionResult> GenerateSalesOrderPdf(int id)
    {
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.OrderLines)
                .ThenInclude(ol => ol.StockItem)
            .FirstOrDefaultAsync(so => so.Id == id);

        if (salesOrder == null)
        {
            return NotFound();
        }

        // Simple PDF generation (placeholder - would need a PDF library like QuestPDF or iTextSharp)
        var pdfContent = $"SALES ORDER #{salesOrder.OrderNumber}\n\n" +
                        $"Date: {salesOrder.OrderDate:yyyy-MM-dd}\n" +
                        $"Customer: {salesOrder.Customer.Name}\n" +
                        $"Status: {salesOrder.Status}\n\n" +
                        $"Items:\n";

        foreach (var line in salesOrder.OrderLines)
        {
            pdfContent += $"{line.StockItem.Name} - Qty: {line.Quantity} @ ${line.UnitPrice} = ${line.LineTotal}\n";
        }

        pdfContent += $"\nSubtotal: ${salesOrder.SubTotal}\n" +
                     $"Tax: ${salesOrder.TaxAmount}\n" +
                     $"Total: ${salesOrder.TotalAmount}";

        var bytes = System.Text.Encoding.UTF8.GetBytes(pdfContent);
        return File(bytes, "text/plain", $"SalesOrder_{salesOrder.OrderNumber}.txt");
    }

    private bool SalesOrderExists(int id)
    {
        return _context.SalesOrders.Any(e => e.Id == id);
    }
}

public class PaymentLinkRequest
{
    public string PaymentMethod { get; set; } = "stripe";
    public string? Currency { get; set; }
}

public class PaymentLinkResponse
{
    public string PaymentLink { get; set; } = string.Empty;
    public string PaymentToken { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}
