# SimpleAccounts

A modern, full-featured accounts management system built with ASP.NET Core and Blazor.

## Features

### Core Functionality
- **Stock Item Management** - Track inventory with warehouse locations, quantities, and pricing
- **Sales Orders** - Create and manage sales orders with automatic tax calculation
- **Bill of Materials (BOM)** - Define product assemblies and component requirements
- **Customer Management** - Maintain customer records with contact details and credit limits
- **Bank Account Management** - Track bank accounts for customers and the business
- **Quote Generation** - Create quotes with PDF export capability
- **Quote to Order Conversion** - Convert quotes to sales orders with one click
- **Payment Link Generation** - Generate secure payment links for orders
- **Warehouse Management** - Monitor stock levels and warehouse locations

### Tax Support
- **UK VAT** - 20% Value Added Tax
- **USA Sales Tax** - 7.5% average sales tax (configurable by state)

### Payment Integration
- **Stripe** - Credit card processing
- **WorldPay** - Payment gateway integration
- **PayPal** - PayPal payment processing

### Security & Access Control
- **ASP.NET Core Identity** - User authentication and authorization
- **Role-Based Access Control** - Pre-configured roles:
  - Admin - Full system access
  - Manager - Management functions
  - SalesUser - Sales and customer management
  - WarehouseUser - Inventory management
  - Accountant - Financial operations

## Technology Stack

- **Backend**: ASP.NET Core 10 Web API
- **Frontend**: Blazor Server with MudBlazor Material Design
- **Database**: Entity Framework Core with SQL Server
- **UI Framework**: Bootstrap 5 + MudBlazor
- **Authentication**: ASP.NET Core Identity

## Project Structure

```
SimpleAccounts/
├── src/
│   ├── SimpleAccounts.Web/        # Blazor web application
│   ├── SimpleAccounts.Api/        # Web API backend
│   └── SimpleAccounts.Data/       # Data models and DbContext
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server (or SQL Server LocalDB)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/dotnetappdev/SimpleAccounts.git
   cd SimpleAccounts
   ```

2. **Update connection strings**
   
   Edit `appsettings.json` in both Web and API projects if needed.

3. **Run database migrations**
   ```bash
   cd src/SimpleAccounts.Api
   dotnet ef database update
   ```

4. **Run the applications**
   
   In separate terminals:
   ```bash
   # Run API
   cd src/SimpleAccounts.Api
   dotnet run

   # Run Web
   cd src/SimpleAccounts.Web
   dotnet run
   ```

5. **Access the application**
   - Web UI: https://localhost:5001
   - API: https://localhost:7001

### Test Users

The system includes pre-seeded test users:

| Email | Password | Role |
|-------|----------|------|
| admin@simpleaccounts.com | Admin@123 | Admin |
| manager@simpleaccounts.com | Test@123 | Manager |
| sales@simpleaccounts.com | Test@123 | SalesUser |
| warehouse@simpleaccounts.com | Test@123 | WarehouseUser |
| accountant@simpleaccounts.com | Test@123 | Accountant |

### Sample Data

The system automatically seeds sample data including:
- 5 stock items (widgets and components)
- 4 customers (UK and USA based)
- 3 bank accounts

## API Endpoints

### Stock Items
- `GET /api/stockitems` - List all stock items
- `GET /api/stockitems/{id}` - Get stock item details
- `POST /api/stockitems` - Create new stock item
- `PUT /api/stockitems/{id}` - Update stock item
- `DELETE /api/stockitems/{id}` - Delete stock item

### Customers
- `GET /api/customers` - List all customers
- `GET /api/customers/{id}` - Get customer details
- `POST /api/customers` - Create new customer
- `PUT /api/customers/{id}` - Update customer
- `DELETE /api/customers/{id}` - Delete customer

### Sales Orders
- `GET /api/salesorders` - List all sales orders
- `GET /api/salesorders/{id}` - Get sales order details
- `POST /api/salesorders` - Create new sales order
- `PUT /api/salesorders/{id}` - Update sales order
- `DELETE /api/salesorders/{id}` - Delete sales order

### Quotes
- `GET /api/quotes` - List all quotes
- `GET /api/quotes/{id}` - Get quote details
- `GET /api/quotes/{id}/pdf` - Generate quote PDF
- `POST /api/quotes` - Create new quote
- `PUT /api/quotes/{id}` - Update quote
- `DELETE /api/quotes/{id}` - Delete quote

### Bill of Materials
- `GET /api/billofmaterials` - List all BOMs
- `GET /api/billofmaterials/{id}` - Get BOM details
- `POST /api/billofmaterials` - Create new BOM
- `PUT /api/billofmaterials/{id}` - Update BOM
- `DELETE /api/billofmaterials/{id}` - Delete BOM

### Bank Accounts
- `GET /api/bankaccounts` - List all bank accounts
- `GET /api/bankaccounts/{id}` - Get bank account details
- `POST /api/bankaccounts` - Create new bank account
- `PUT /api/bankaccounts/{id}` - Update bank account
- `DELETE /api/bankaccounts/{id}` - Delete bank account

### Payments
- `GET /api/payments/methods` - List available payment methods
- `POST /api/payments/stripe` - Process Stripe payment
- `POST /api/payments/worldpay` - Process WorldPay payment
- `POST /api/payments/paypal` - Process PayPal payment

## Development

### Database Migrations

Create a new migration:
```bash
cd src/SimpleAccounts.Api
dotnet ef migrations add MigrationName
```

Update database:
```bash
dotnet ef database update
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.
