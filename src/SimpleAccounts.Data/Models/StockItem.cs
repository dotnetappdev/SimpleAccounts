namespace SimpleAccounts.Data.Models;

public class StockItem
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }
    public string? WarehouseLocation { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedDate { get; set; }
}
