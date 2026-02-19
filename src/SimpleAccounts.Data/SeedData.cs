using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleAccounts.Data.Models;

namespace SimpleAccounts.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is created
        await context.Database.MigrateAsync();

        // Seed Roles
        string[] roleNames = { "Admin", "Manager", "SalesUser", "WarehouseUser", "Accountant" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Seed Admin User
        var adminEmail = "admin@simpleaccounts.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "Admin@123");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Seed Test Users
        var testUsers = new[]
        {
            new { Email = "manager@simpleaccounts.com", FirstName = "John", LastName = "Manager", Role = "Manager" },
            new { Email = "sales@simpleaccounts.com", FirstName = "Jane", LastName = "Sales", Role = "SalesUser" },
            new { Email = "warehouse@simpleaccounts.com", FirstName = "Bob", LastName = "Warehouse", Role = "WarehouseUser" },
            new { Email = "accountant@simpleaccounts.com", FirstName = "Alice", LastName = "Accountant", Role = "Accountant" }
        };

        foreach (var testUser in testUsers)
        {
            var user = await userManager.FindByEmailAsync(testUser.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = testUser.Email,
                    Email = testUser.Email,
                    FirstName = testUser.FirstName,
                    LastName = testUser.LastName,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, "Test@123");
                await userManager.AddToRoleAsync(user, testUser.Role);
            }
        }

        // Seed Stock Items
        if (!await context.StockItems.AnyAsync())
        {
            var stockItems = new[]
            {
                new StockItem { Code = "WIDGET001", Name = "Standard Widget", Description = "Standard widget for general use", UnitPrice = 10.00m, CostPrice = 5.00m, QuantityOnHand = 100, ReorderLevel = 20, WarehouseLocation = "A-1-1" },
                new StockItem { Code = "WIDGET002", Name = "Premium Widget", Description = "Premium quality widget", UnitPrice = 25.00m, CostPrice = 12.50m, QuantityOnHand = 50, ReorderLevel = 10, WarehouseLocation = "A-1-2" },
                new StockItem { Code = "PART001", Name = "Widget Component A", Description = "Component for widget assembly", UnitPrice = 2.50m, CostPrice = 1.25m, QuantityOnHand = 500, ReorderLevel = 100, WarehouseLocation = "B-2-1" },
                new StockItem { Code = "PART002", Name = "Widget Component B", Description = "Component for widget assembly", UnitPrice = 3.00m, CostPrice = 1.50m, QuantityOnHand = 400, ReorderLevel = 80, WarehouseLocation = "B-2-2" },
                new StockItem { Code = "TOOL001", Name = "Assembly Tool", Description = "Tool for widget assembly", UnitPrice = 50.00m, CostPrice = 25.00m, QuantityOnHand = 10, ReorderLevel = 2, WarehouseLocation = "C-3-1" }
            };
            context.StockItems.AddRange(stockItems);
            await context.SaveChangesAsync();
        }

        // Seed Customers
        if (!await context.Customers.AnyAsync())
        {
            var customers = new[]
            {
                new Customer { Code = "CUST001", Name = "Acme Corporation", Email = "orders@acme.com", Phone = "555-0100", Address = "123 Industrial Way", City = "Manchester", State = "", PostalCode = "M1 1AA", Country = "UK", CreditLimit = 10000m, Balance = 0m },
                new Customer { Code = "CUST002", Name = "Global Supplies Inc", Email = "purchasing@globalsupplies.com", Phone = "555-0200", Address = "456 Trade Street", City = "New York", State = "NY", PostalCode = "10001", Country = "USA", CreditLimit = 25000m, Balance = 0m },
                new Customer { Code = "CUST003", Name = "British Manufacturing Ltd", Email = "procurement@britman.co.uk", Phone = "555-0300", Address = "789 Factory Lane", City = "Birmingham", State = "", PostalCode = "B1 1BB", Country = "UK", CreditLimit = 15000m, Balance = 0m },
                new Customer { Code = "CUST004", Name = "Tech Solutions LLC", Email = "orders@techsolutions.com", Phone = "555-0400", Address = "321 Silicon Valley", City = "San Francisco", State = "CA", PostalCode = "94105", Country = "USA", CreditLimit = 20000m, Balance = 0m }
            };
            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();
        }

        // Seed Bank Accounts
        if (!await context.BankAccounts.AnyAsync())
        {
            var customer1 = await context.Customers.FirstAsync(c => c.Code == "CUST001");
            var customer2 = await context.Customers.FirstAsync(c => c.Code == "CUST002");

            var bankAccounts = new[]
            {
                new BankAccount { AccountName = "Business Account", AccountNumber = "12345678", BankName = "Barclays Bank", SortCode = "20-00-00", CustomerId = customer1.Id, Balance = 5000m, Currency = "GBP" },
                new BankAccount { AccountName = "Operating Account", AccountNumber = "87654321", BankName = "Chase Bank", RoutingNumber = "021000021", CustomerId = customer2.Id, Balance = 10000m, Currency = "USD" },
                new BankAccount { AccountName = "Main Business Account", AccountNumber = "11223344", BankName = "HSBC", SortCode = "40-00-00", CustomerId = null, Balance = 50000m, Currency = "GBP" }
            };
            context.BankAccounts.AddRange(bankAccounts);
            await context.SaveChangesAsync();
        }
    }
}
