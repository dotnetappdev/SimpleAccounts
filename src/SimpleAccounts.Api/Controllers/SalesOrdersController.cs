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

    private bool SalesOrderExists(int id)
    {
        return _context.SalesOrders.Any(e => e.Id == id);
    }
}
