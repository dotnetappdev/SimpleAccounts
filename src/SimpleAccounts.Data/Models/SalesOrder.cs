namespace SimpleAccounts.Data.Models;

public class SalesOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveryDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Confirmed, Shipped, Delivered, Cancelled
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedDate { get; set; }
    
    public ICollection<SalesOrderLine> OrderLines { get; set; } = new List<SalesOrderLine>();
}

public class SalesOrderLine
{
    public int Id { get; set; }
    public int SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = null!;
    public int StockItemId { get; set; }
    public StockItem StockItem { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal LineTotal { get; set; }
}
