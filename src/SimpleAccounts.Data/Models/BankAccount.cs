namespace SimpleAccounts.Data.Models;

public class BankAccount
{
    public int Id { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? SortCode { get; set; }
    public string? SwiftCode { get; set; }
    public string? RoutingNumber { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "GBP";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedDate { get; set; }
}
