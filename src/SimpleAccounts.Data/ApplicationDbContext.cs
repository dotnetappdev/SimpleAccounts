using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleAccounts.Data.Models;

namespace SimpleAccounts.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<StockItem> StockItems { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<SalesOrderLine> SalesOrderLines { get; set; }
    public DbSet<BillOfMaterials> BillOfMaterials { get; set; }
    public DbSet<BillOfMaterialsLine> BillOfMaterialsLines { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<QuoteLine> QuoteLines { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure decimal precision for financial fields
        modelBuilder.Entity<StockItem>()
            .Property(s => s.UnitPrice)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<StockItem>()
            .Property(s => s.CostPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Customer>()
            .Property(c => c.CreditLimit)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<Customer>()
            .Property(c => c.Balance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SalesOrder>()
            .Property(so => so.SubTotal)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<SalesOrder>()
            .Property(so => so.TaxAmount)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<SalesOrder>()
            .Property(so => so.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SalesOrderLine>()
            .Property(sol => sol.UnitPrice)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<SalesOrderLine>()
            .Property(sol => sol.Discount)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<SalesOrderLine>()
            .Property(sol => sol.LineTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Quote>()
            .Property(q => q.SubTotal)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<Quote>()
            .Property(q => q.TaxAmount)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<Quote>()
            .Property(q => q.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<QuoteLine>()
            .Property(ql => ql.UnitPrice)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<QuoteLine>()
            .Property(ql => ql.Discount)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<QuoteLine>()
            .Property(ql => ql.LineTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BankAccount>()
            .Property(ba => ba.Balance)
            .HasPrecision(18, 2);

        // Configure relationships
        modelBuilder.Entity<SalesOrder>()
            .HasOne(so => so.Customer)
            .WithMany(c => c.SalesOrders)
            .HasForeignKey(so => so.CustomerId);

        modelBuilder.Entity<BankAccount>()
            .HasOne(ba => ba.Customer)
            .WithMany(c => c.BankAccounts)
            .HasForeignKey(ba => ba.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
