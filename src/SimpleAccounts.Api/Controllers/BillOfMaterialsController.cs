using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleAccounts.Data;
using SimpleAccounts.Data.Models;

namespace SimpleAccounts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillOfMaterialsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BillOfMaterialsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BillOfMaterials>>> GetBillOfMaterials()
    {
        return await _context.BillOfMaterials
            .Include(bom => bom.FinishedProduct)
            .Include(bom => bom.BomLines)
                .ThenInclude(bl => bl.Component)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BillOfMaterials>> GetBillOfMaterial(int id)
    {
        var billOfMaterial = await _context.BillOfMaterials
            .Include(bom => bom.FinishedProduct)
            .Include(bom => bom.BomLines)
                .ThenInclude(bl => bl.Component)
            .FirstOrDefaultAsync(bom => bom.Id == id);

        if (billOfMaterial == null)
        {
            return NotFound();
        }

        return billOfMaterial;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BillOfMaterials>> PostBillOfMaterial(BillOfMaterials billOfMaterial)
    {
        billOfMaterial.CreatedDate = DateTime.UtcNow;
        _context.BillOfMaterials.Add(billOfMaterial);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBillOfMaterial), new { id = billOfMaterial.Id }, billOfMaterial);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> PutBillOfMaterial(int id, BillOfMaterials billOfMaterial)
    {
        if (id != billOfMaterial.Id)
        {
            return BadRequest();
        }

        billOfMaterial.LastModifiedDate = DateTime.UtcNow;
        _context.Entry(billOfMaterial).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BillOfMaterialExists(id))
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
    public async Task<IActionResult> DeleteBillOfMaterial(int id)
    {
        var billOfMaterial = await _context.BillOfMaterials.FindAsync(id);
        if (billOfMaterial == null)
        {
            return NotFound();
        }

        _context.BillOfMaterials.Remove(billOfMaterial);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BillOfMaterialExists(int id)
    {
        return _context.BillOfMaterials.Any(e => e.Id == id);
    }
}
