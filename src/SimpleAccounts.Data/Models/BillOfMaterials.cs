namespace SimpleAccounts.Data.Models;

public class BillOfMaterials
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int FinishedProductId { get; set; }
    public StockItem FinishedProduct { get; set; } = null!;
    public int Quantity { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedDate { get; set; }
    
    public ICollection<BillOfMaterialsLine> BomLines { get; set; } = new List<BillOfMaterialsLine>();
}

public class BillOfMaterialsLine
{
    public int Id { get; set; }
    public int BillOfMaterialsId { get; set; }
    public BillOfMaterials BillOfMaterials { get; set; } = null!;
    public int ComponentId { get; set; }
    public StockItem Component { get; set; } = null!;
    public int QuantityRequired { get; set; }
    public string? Notes { get; set; }
}
