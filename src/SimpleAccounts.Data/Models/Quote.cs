namespace SimpleAccounts.Data.Models;

public class Quote
{
    public int Id { get; set; }
    public string QuoteNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime QuoteDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Sent, Accepted, Rejected, Expired
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedDate { get; set; }
    
    public ICollection<QuoteLine> QuoteLines { get; set; } = new List<QuoteLine>();
}

public class QuoteLine
{
    public int Id { get; set; }
    public int QuoteId { get; set; }
    public Quote Quote { get; set; } = null!;
    public int StockItemId { get; set; }
    public StockItem StockItem { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal LineTotal { get; set; }
}
