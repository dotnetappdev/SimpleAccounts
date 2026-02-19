# Deployment Guide

This guide covers deploying SimpleAccounts to various environments.

## Prerequisites

- .NET 10 SDK or runtime
- SQL Server database
- Web server (IIS, Nginx, or Azure App Service)

## Local Development Deployment

### 1. Clone and Build

```bash
git clone https://github.com/dotnetappdev/SimpleAccounts.git
cd SimpleAccounts
dotnet build
```

### 2. Configure Database

Edit `src/SimpleAccounts.Api/appsettings.json` and `src/SimpleAccounts.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SimpleAccounts;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 3. Run Applications

**Terminal 1 - API:**
```bash
cd src/SimpleAccounts.Api
dotnet run
```

**Terminal 2 - Web:**
```bash
cd src/SimpleAccounts.Web
dotnet run
```

### 4. Access the Application

- Web UI: https://localhost:5001
- API: https://localhost:7001
- OpenAPI/Swagger: https://localhost:7001/openapi/v1.json

---

## Production Deployment

### Azure App Service Deployment

#### 1. Publish the Applications

```bash
# Publish API
cd src/SimpleAccounts.Api
dotnet publish -c Release -o ./publish

# Publish Web
cd ../SimpleAccounts.Web
dotnet publish -c Release -o ./publish
```

#### 2. Create Azure Resources

Using Azure CLI:

```bash
# Create resource group
az group create --name SimpleAccountsRG --location eastus

# Create SQL Server
az sql server create \
  --name simpleaccounts-sql \
  --resource-group SimpleAccountsRG \
  --location eastus \
  --admin-user sqladmin \
  --admin-password YourStrongPassword123!

# Create SQL Database
az sql db create \
  --resource-group SimpleAccountsRG \
  --server simpleaccounts-sql \
  --name SimpleAccounts \
  --service-objective S0

# Create App Service Plan
az appservice plan create \
  --name SimpleAccountsPlan \
  --resource-group SimpleAccountsRG \
  --sku B1 \
  --is-linux

# Create Web App for API
az webapp create \
  --resource-group SimpleAccountsRG \
  --plan SimpleAccountsPlan \
  --name simpleaccounts-api \
  --runtime "DOTNETCORE:10.0"

# Create Web App for Web UI
az webapp create \
  --resource-group SimpleAccountsRG \
  --plan SimpleAccountsPlan \
  --name simpleaccounts-web \
  --runtime "DOTNETCORE:10.0"
```

#### 3. Configure Connection Strings

```bash
# Get SQL connection string
az sql db show-connection-string \
  --client ado.net \
  --server simpleaccounts-sql \
  --name SimpleAccounts

# Set connection string for API
az webapp config connection-string set \
  --resource-group SimpleAccountsRG \
  --name simpleaccounts-api \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:simpleaccounts-sql.database.windows.net,1433;Initial Catalog=SimpleAccounts;Persist Security Info=False;User ID=sqladmin;Password=YourStrongPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Set connection string for Web
az webapp config connection-string set \
  --resource-group SimpleAccountsRG \
  --name simpleaccounts-web \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:simpleaccounts-sql.database.windows.net,1433;Initial Catalog=SimpleAccounts;Persist Security Info=False;User ID=sqladmin;Password=YourStrongPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

#### 4. Deploy Applications

```bash
# Deploy API
cd src/SimpleAccounts.Api/publish
zip -r api.zip .
az webapp deployment source config-zip \
  --resource-group SimpleAccountsRG \
  --name simpleaccounts-api \
  --src api.zip

# Deploy Web
cd ../../SimpleAccounts.Web/publish
zip -r web.zip .
az webapp deployment source config-zip \
  --resource-group SimpleAccountsRG \
  --name simpleaccounts-web \
  --src web.zip
```

#### 5. Update API Base URL

Update Web app configuration:

```bash
az webapp config appsettings set \
  --resource-group SimpleAccountsRG \
  --name simpleaccounts-web \
  --settings ApiBaseUrl="https://simpleaccounts-api.azurewebsites.net"
```

#### 6. Configure CORS

Update API CORS settings to allow Web app:

```bash
az webapp cors add \
  --resource-group SimpleAccountsRG \
  --name simpleaccounts-api \
  --allowed-origins "https://simpleaccounts-web.azurewebsites.net"
```

---

### IIS Deployment (Windows Server)

#### 1. Install Prerequisites

- IIS with ASP.NET Core Hosting Bundle
- .NET 10 Runtime
- SQL Server

#### 2. Publish Applications

```bash
dotnet publish src/SimpleAccounts.Api -c Release -o C:\inetpub\SimpleAccounts.Api
dotnet publish src/SimpleAccounts.Web -c Release -o C:\inetpub\SimpleAccounts.Web
```

#### 3. Create IIS Application Pools

```powershell
Import-Module WebAdministration

# Create API App Pool
New-WebAppPool -Name "SimpleAccounts.Api"
Set-ItemProperty IIS:\AppPools\SimpleAccounts.Api -Name managedRuntimeVersion -Value ""

# Create Web App Pool
New-WebAppPool -Name "SimpleAccounts.Web"
Set-ItemProperty IIS:\AppPools\SimpleAccounts.Web -Name managedRuntimeVersion -Value ""
```

#### 4. Create IIS Sites

```powershell
# Create API Site
New-Website -Name "SimpleAccounts.Api" `
  -PhysicalPath "C:\inetpub\SimpleAccounts.Api" `
  -ApplicationPool "SimpleAccounts.Api" `
  -Port 7001

# Create Web Site
New-Website -Name "SimpleAccounts.Web" `
  -PhysicalPath "C:\inetpub\SimpleAccounts.Web" `
  -ApplicationPool "SimpleAccounts.Web" `
  -Port 5001
```

#### 5. Update Configuration

Edit `web.config` in each published folder if needed, or use `appsettings.Production.json`.

---

### Docker Deployment

#### 1. Create Dockerfiles

**API Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/SimpleAccounts.Api/SimpleAccounts.Api.csproj", "SimpleAccounts.Api/"]
COPY ["src/SimpleAccounts.Data/SimpleAccounts.Data.csproj", "SimpleAccounts.Data/"]
RUN dotnet restore "SimpleAccounts.Api/SimpleAccounts.Api.csproj"
COPY src/ .
WORKDIR "/src/SimpleAccounts.Api"
RUN dotnet build "SimpleAccounts.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleAccounts.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleAccounts.Api.dll"]
```

**Web Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/SimpleAccounts.Web/SimpleAccounts.Web.csproj", "SimpleAccounts.Web/"]
COPY ["src/SimpleAccounts.Data/SimpleAccounts.Data.csproj", "SimpleAccounts.Data/"]
RUN dotnet restore "SimpleAccounts.Web/SimpleAccounts.Web.csproj"
COPY src/ .
WORKDIR "/src/SimpleAccounts.Web"
RUN dotnet build "SimpleAccounts.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleAccounts.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleAccounts.Web.dll"]
```

#### 2. Docker Compose

**docker-compose.yml:**
```yaml
version: '3.8'

services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password123
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

  api:
    build:
      context: .
      dockerfile: Dockerfile.Api
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;Database=SimpleAccounts;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True
    ports:
      - "7001:80"
    depends_on:
      - db

  web:
    build:
      context: .
      dockerfile: Dockerfile.Web
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;Database=SimpleAccounts;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True
      - ApiBaseUrl=http://api
    ports:
      - "5001:80"
    depends_on:
      - api

volumes:
  sqldata:
```

#### 3. Run with Docker Compose

```bash
docker-compose up -d
```

---

## Post-Deployment Steps

### 1. Verify Database

Ensure the database was created and seeded:
- Check that all tables exist
- Verify seed data (users, roles, sample data)

### 2. Test Authentication

Login with test credentials:
- Email: `admin@simpleaccounts.com`
- Password: `Admin@123`

### 3. Change Default Passwords

**Important:** Change all default passwords in production!

### 4. Configure SSL/TLS

Ensure HTTPS is properly configured:
- Install SSL certificates
- Redirect HTTP to HTTPS
- Update connection strings to use encryption

### 5. Configure Monitoring

Set up application monitoring:
- Application Insights (Azure)
- ELK Stack
- Custom logging solution

### 6. Configure Backups

Set up automated database backups:
- Azure SQL automated backups
- SQL Server maintenance plans
- Regular backup testing

---

## Environment Variables

Key environment variables for production:

```bash
# Connection Strings
ConnectionStrings__DefaultConnection="..."

# API Configuration
ApiBaseUrl="https://api.yourdomain.com"

# CORS
AllowedHosts="yourdomain.com"

# Logging
Logging__LogLevel__Default="Warning"
Logging__LogLevel__Microsoft="Warning"
```

---

## Security Checklist

- [ ] Change all default passwords
- [ ] Use environment variables for secrets
- [ ] Enable HTTPS/SSL
- [ ] Configure CORS properly
- [ ] Enable database encryption
- [ ] Set up firewall rules
- [ ] Configure authentication properly
- [ ] Enable audit logging
- [ ] Regular security updates
- [ ] Implement rate limiting

---

## Troubleshooting

### Application Won't Start

1. Check .NET runtime is installed
2. Verify connection strings
3. Check port availability
4. Review application logs

### Database Connection Errors

1. Verify SQL Server is accessible
2. Check firewall rules
3. Validate connection string
4. Test with SQL Server Management Studio

### CORS Errors

1. Verify CORS configuration in API
2. Check allowed origins
3. Ensure proper headers are set

---

## Monitoring and Maintenance

### Health Checks

Add health check endpoints:
- Database connectivity
- External service availability
- Disk space
- Memory usage

### Log Aggregation

Recommended logging solutions:
- Azure Application Insights
- ELK Stack (Elasticsearch, Logstash, Kibana)
- Seq
- Splunk

### Performance Monitoring

Monitor:
- Response times
- Database query performance
- Memory usage
- CPU utilization

---

## Scaling Considerations

### Horizontal Scaling

- Use Azure App Service scaling
- Load balancers
- Multiple instances

### Database Scaling

- Read replicas
- Database sharding
- Caching (Redis)

### CDN

- Use CDN for static assets
- Azure CDN
- Cloudflare

---

## Support

For issues or questions:
- GitHub Issues: https://github.com/dotnetappdev/SimpleAccounts/issues
- Documentation: See `/docs` folder
