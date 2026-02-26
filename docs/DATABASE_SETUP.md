# Database Setup Guide

This guide explains how to set up the database for SimpleAccounts.

## Prerequisites

You need one of the following:
- SQL Server 2019 or later
- SQL Server Express (free)
- SQL Server LocalDB (included with Visual Studio)
- Azure SQL Database

## Connection String

The default connection string in `appsettings.json` is configured for SQL Server LocalDB:

```
Server=(localdb)\\mssqllocaldb;Database=SimpleAccounts;Trusted_Connection=True;MultipleActiveResultSets=true
```

### For SQL Server Express

Update the connection string to:
```
Server=localhost\\SQLEXPRESS;Database=SimpleAccounts;Trusted_Connection=True;MultipleActiveResultSets=true
```

### For SQL Server with authentication

Update the connection string to:
```
Server=YOUR_SERVER;Database=SimpleAccounts;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True
```

### For Azure SQL Database

Update the connection string to:
```
Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=SimpleAccounts;Persist Security Info=False;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Creating the Database

### Method 1: Automatic (Recommended)

The application will automatically create and seed the database on first run. Just start the application:

```bash
cd src/SimpleAccounts.Api
dotnet run
```

or

```bash
cd src/SimpleAccounts.Web
dotnet run
```

The `SeedData.InitializeAsync` method in `Program.cs` will:
1. Create the database if it doesn't exist
2. Apply all migrations
3. Seed initial data (users, roles, sample customers, stock items, etc.)

### Method 2: Manual Migration

If you prefer to manually create the database:

```bash
cd src/SimpleAccounts.Api
dotnet ef database update
```

This will:
1. Create the database
2. Create all tables and relationships
3. Apply the InitialCreate migration

Note: You'll still need to run the application once to seed the initial data.

## Seeded Data

The application automatically seeds the following data:

### Users and Roles

| Role | Email | Password | Permissions |
|------|-------|----------|-------------|
| Admin | admin@simpleaccounts.com | Admin@123 | Full system access |
| Manager | manager@simpleaccounts.com | Test@123 | Management functions |
| SalesUser | sales@simpleaccounts.com | Test@123 | Sales and customer management |
| WarehouseUser | warehouse@simpleaccounts.com | Test@123 | Inventory management |
| Accountant | accountant@simpleaccounts.com | Test@123 | Financial operations |

### Sample Data

- **5 Stock Items**: Widgets and components with pricing and inventory
- **4 Customers**: Mix of UK and USA customers
- **3 Bank Accounts**: Sample business and customer accounts

## Database Schema

The database includes the following main tables:

### Identity Tables (ASP.NET Core Identity)
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetRoleClaims
- AspNetUserLogins
- AspNetUserTokens

### Business Tables
- **StockItems**: Product inventory
- **Customers**: Customer information
- **SalesOrders**: Sales order headers
- **SalesOrderLines**: Sales order line items
- **BillOfMaterials**: Bill of materials headers
- **BillOfMaterialsLines**: BOM components
- **BankAccounts**: Bank account information
- **Quotes**: Quote headers
- **QuoteLines**: Quote line items

## Troubleshooting

### Connection Failed

If you get a connection error:
1. Verify SQL Server is running
2. Check the connection string in `appsettings.json`
3. Ensure your user has permission to create databases

### LocalDB Not Found

If using LocalDB and getting "cannot connect" errors:
1. Install SQL Server Express LocalDB
2. Or use SQL Server Express instead
3. Update the connection string accordingly

### Migration Errors

If you encounter migration errors:
1. Delete the `Migrations` folder
2. Run `dotnet ef migrations add InitialCreate` again
3. Run `dotnet ef database update`

### Reset Database

To completely reset the database:

```bash
cd src/SimpleAccounts.Api
dotnet ef database drop
dotnet ef database update
```

Then restart the application to re-seed the data.

## Production Deployment

For production:

1. **Never use the default connection string**
2. **Store connection strings in environment variables or Azure Key Vault**
3. **Use a dedicated SQL Server instance**
4. **Change all default passwords**
5. **Remove or disable seed data for test users**
6. **Enable SSL/TLS encryption for connections**
7. **Implement proper backup procedures**

### Example Production Configuration

In `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "#{ConnectionString}#"
  }
}
```

Then set the environment variable or use Azure App Service configuration.
