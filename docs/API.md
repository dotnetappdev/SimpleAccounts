# API Documentation

SimpleAccounts provides a RESTful API for all business operations.

## Base URL

Development: `https://localhost:7001/api`

## Authentication

The API uses JWT (JSON Web Token) bearer authentication. Protected endpoints require:
- Valid JWT token in the Authorization header
- Appropriate role membership

### Getting a Token

**Login:**
```
POST /api/auth/login

{
  "email": "admin@simpleaccounts.com",
  "password": "Admin@123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "admin@simpleaccounts.com",
  "firstName": "Admin",
  "lastName": "User",
  "roles": ["Admin"],
  "expiresAt": "2024-01-01T13:00:00Z"
}
```

**Using the Token:**

Include the token in the Authorization header for all protected requests:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Token Refresh:**
```
POST /api/auth/refresh

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

Returns a new token with extended expiry.

### JWT Claims

The JWT token includes the following claims:
- **NameIdentifier**: User ID
- **Name**: Username
- **Email**: User email address
- **FirstName**: User's first name
- **LastName**: User's last name
- **Role**: User role(s) for authorization
- Custom claims from the database

### Roles

- **Admin**: Full access to all endpoints
- **Manager**: Access to most management functions
- **SalesUser**: Customer and sales operations
- **WarehouseUser**: Inventory management
- **Accountant**: Financial operations

### Token Configuration

- **Expiry**: 60 minutes (configurable in appsettings.json)
- **Algorithm**: HMACSHA256
- **Issuer**: SimpleAccountsAPI
- **Audience**: SimpleAccountsWeb

## Endpoints

### Stock Items

#### List All Stock Items
```
GET /api/stockitems
```

**Response:**
```json
[
  {
    "id": 1,
    "code": "WIDGET001",
    "name": "Standard Widget",
    "description": "Standard widget for general use",
    "unitPrice": 10.00,
    "costPrice": 5.00,
    "quantityOnHand": 100,
    "reorderLevel": 20,
    "warehouseLocation": "A-1-1",
    "createdDate": "2024-01-01T00:00:00Z",
    "lastModifiedDate": null
  }
]
```

#### Get Stock Item
```
GET /api/stockitems/{id}
```

#### Create Stock Item
```
POST /api/stockitems
Authorization: Required (Admin, Manager, WarehouseUser)

{
  "code": "WIDGET001",
  "name": "Standard Widget",
  "description": "Standard widget for general use",
  "unitPrice": 10.00,
  "costPrice": 5.00,
  "quantityOnHand": 100,
  "reorderLevel": 20,
  "warehouseLocation": "A-1-1"
}
```

#### Update Stock Item
```
PUT /api/stockitems/{id}
Authorization: Required (Admin, Manager, WarehouseUser)
```

#### Delete Stock Item
```
DELETE /api/stockitems/{id}
Authorization: Required (Admin, Manager)
```

---

### Customers

#### List All Customers
```
GET /api/customers
```

**Response:**
```json
[
  {
    "id": 1,
    "code": "CUST001",
    "name": "Acme Corporation",
    "email": "orders@acme.com",
    "phone": "555-0100",
    "address": "123 Industrial Way",
    "city": "Manchester",
    "state": "",
    "postalCode": "M1 1AA",
    "country": "UK",
    "creditLimit": 10000.00,
    "balance": 0.00,
    "bankAccounts": [],
    "salesOrders": []
  }
]
```

#### Get Customer
```
GET /api/customers/{id}
```

Includes related bank accounts and sales orders.

#### Create Customer
```
POST /api/customers
Authorization: Required (Admin, Manager, SalesUser)

{
  "code": "CUST001",
  "name": "Acme Corporation",
  "email": "orders@acme.com",
  "phone": "555-0100",
  "address": "123 Industrial Way",
  "city": "Manchester",
  "postalCode": "M1 1AA",
  "country": "UK",
  "creditLimit": 10000.00
}
```

#### Update Customer
```
PUT /api/customers/{id}
Authorization: Required (Admin, Manager, SalesUser)
```

#### Delete Customer
```
DELETE /api/customers/{id}
Authorization: Required (Admin, Manager)
```

---

### Sales Orders

#### List All Sales Orders
```
GET /api/salesorders
```

Includes customer and order line details.

#### Get Sales Order
```
GET /api/salesorders/{id}
```

#### Create Sales Order
```
POST /api/salesorders
Authorization: Required (Admin, Manager, SalesUser)

{
  "customerId": 1,
  "orderDate": "2024-01-01T00:00:00Z",
  "deliveryDate": "2024-01-15T00:00:00Z",
  "status": "Draft",
  "notes": "Rush order",
  "orderLines": [
    {
      "stockItemId": 1,
      "quantity": 10,
      "unitPrice": 10.00,
      "discount": 0.00,
      "lineTotal": 100.00
    }
  ]
}
```

**Note:** Tax is automatically calculated based on customer's country.

#### Update Sales Order
```
PUT /api/salesorders/{id}
Authorization: Required (Admin, Manager, SalesUser)
```

#### Delete Sales Order
```
DELETE /api/salesorders/{id}
Authorization: Required (Admin, Manager)
```

#### Generate Sales Order PDF
```
GET /api/salesorders/{id}/pdf
Authorization: Required (Admin, Manager, SalesUser)
```

Returns a formatted sales order document with all order details and line items.

#### Generate Payment Link
```
POST /api/salesorders/{id}/generate-payment-link
Authorization: Required (Admin, Manager, SalesUser)

{
  "paymentMethod": "stripe",
  "currency": "GBP"
}
```

**Response:**
```json
{
  "paymentLink": "https://localhost:5001/payment/abc123...",
  "paymentToken": "abc123...",
  "orderId": 1,
  "amount": 150.00,
  "currency": "GBP",
  "expiresAt": "2024-01-02T00:00:00Z",
  "paymentMethod": "stripe"
}
```

Generates a unique payment link for the sales order with support for Stripe, WorldPay, and PayPal.

---

### Quotes

#### List All Quotes
```
GET /api/quotes
```

#### Get Quote
```
GET /api/quotes/{id}
```

#### Create Quote
```
POST /api/quotes
Authorization: Required (Admin, Manager, SalesUser)

{
  "customerId": 1,
  "quoteDate": "2024-01-01T00:00:00Z",
  "expiryDate": "2024-02-01T00:00:00Z",
  "status": "Draft",
  "notes": "Standard quote",
  "quoteLines": [
    {
      "stockItemId": 1,
      "quantity": 10,
      "unitPrice": 10.00,
      "discount": 0.00,
      "lineTotal": 100.00
    }
  ]
}
```

#### Generate Quote PDF
```
GET /api/quotes/{id}/pdf
Authorization: Required (Admin, Manager, SalesUser)
```

Returns a text file representation of the quote (PDF generation library integration needed for full PDF support).

#### Update Quote
```
PUT /api/quotes/{id}
Authorization: Required (Admin, Manager, SalesUser)
```

#### Delete Quote
```
DELETE /api/quotes/{id}
Authorization: Required (Admin, Manager)
```

#### Convert Quote to Sales Order
```
POST /api/quotes/{id}/convert-to-order
Authorization: Required (Admin, Manager, SalesUser)
```

**Response:**
```json
{
  "id": 1,
  "orderNumber": "SO-20240101120000",
  "customerId": 1,
  "orderDate": "2024-01-01T12:00:00Z",
  "status": "Draft",
  "subTotal": 100.00,
  "taxAmount": 20.00,
  "totalAmount": 120.00,
  "notes": "Converted from Quote #Q-001",
  "orderLines": [...]
}
```

Converts a quote into a sales order, preserving all line items and pricing. Updates the quote status to "Accepted".

---

### Bill of Materials

#### List All BOMs
```
GET /api/billofmaterials
```

#### Get BOM
```
GET /api/billofmaterials/{id}
```

#### Create BOM
```
POST /api/billofmaterials
Authorization: Required (Admin, Manager)

{
  "code": "BOM001",
  "name": "Widget Assembly",
  "description": "Complete widget assembly",
  "finishedProductId": 1,
  "quantity": 1,
  "bomLines": [
    {
      "componentId": 3,
      "quantityRequired": 2,
      "notes": "Component A"
    },
    {
      "componentId": 4,
      "quantityRequired": 1,
      "notes": "Component B"
    }
  ]
}
```

#### Update BOM
```
PUT /api/billofmaterials/{id}
Authorization: Required (Admin, Manager)
```

#### Delete BOM
```
DELETE /api/billofmaterials/{id}
Authorization: Required (Admin, Manager)
```

---

### Bank Accounts

#### List All Bank Accounts
```
GET /api/bankaccounts
```

#### Get Bank Account
```
GET /api/bankaccounts/{id}
```

#### Create Bank Account
```
POST /api/bankaccounts
Authorization: Required (Admin, Manager, Accountant)

{
  "accountName": "Business Account",
  "accountNumber": "12345678",
  "bankName": "Barclays Bank",
  "sortCode": "20-00-00",
  "customerId": 1,
  "balance": 5000.00,
  "currency": "GBP"
}
```

#### Update Bank Account
```
PUT /api/bankaccounts/{id}
Authorization: Required (Admin, Manager, Accountant)
```

#### Delete Bank Account
```
DELETE /api/bankaccounts/{id}
Authorization: Required (Admin, Manager)
```

---

### Payments

#### List Payment Methods
```
GET /api/payments/methods
```

**Response:**
```json
[
  { "name": "Stripe", "enabled": true },
  { "name": "WorldPay", "enabled": true },
  { "name": "PayPal", "enabled": true }
]
```

#### Process Stripe Payment
```
POST /api/payments/stripe

{
  "amount": 100.00,
  "currency": "GBP",
  "customerEmail": "customer@example.com",
  "description": "Order payment",
  "orderId": 1
}
```

**Response:**
```json
{
  "success": true,
  "transactionId": "STRIPE_abc123...",
  "message": "Payment processed successfully via Stripe"
}
```

#### Process WorldPay Payment
```
POST /api/payments/worldpay
```

Same request/response format as Stripe.

#### Process PayPal Payment
```
POST /api/payments/paypal
```

Same request/response format as Stripe.

**Note:** Payment endpoints are placeholders. Integration with actual payment providers requires:
1. Payment provider SDK installation (Stripe.NET, PayPal SDK, etc.)
2. API keys configuration
3. Webhook handling for payment confirmations

---

## Error Responses

### 400 Bad Request
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."]
  }
}
```

### 401 Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### 403 Forbidden
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403
}
```

### 404 Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404
}
```

---

## Rate Limiting

Currently, no rate limiting is implemented. For production deployment, consider implementing rate limiting using:
- `AspNetCoreRateLimit` NuGet package
- API Gateway (Azure API Management, AWS API Gateway, etc.)

## CORS

The API includes CORS configuration to allow requests from the Blazor frontend. The default allowed origins are:
- `https://localhost:5001`
- `http://localhost:5000`

Update `Program.cs` to add additional origins for production deployment.
