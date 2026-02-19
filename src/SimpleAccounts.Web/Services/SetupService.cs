using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace SimpleAccounts.Web.Services;

public class SetupService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly string _webAppSettingsPath;
    private readonly string _webAppSettingsDevPath;
    private readonly string _apiAppSettingsPath;
    private readonly string _apiAppSettingsDevPath;

    public SetupService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
        
        var webRoot = Path.Combine(_environment.ContentRootPath);
        var apiRoot = Path.Combine(_environment.ContentRootPath, "..", "SimpleAccounts.Api");
        
        _webAppSettingsPath = Path.Combine(webRoot, "appsettings.json");
        _webAppSettingsDevPath = Path.Combine(webRoot, "appsettings.Development.json");
        _apiAppSettingsPath = Path.Combine(apiRoot, "appsettings.json");
        _apiAppSettingsDevPath = Path.Combine(apiRoot, "appsettings.Development.json");
    }

    public bool IsSetupComplete()
    {
        var setupComplete = _configuration.GetValue<bool>("SetupComplete");
        return setupComplete;
    }

    public async Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string BuildConnectionString(DatabaseConnectionInfo info)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = info.ServerName,
            InitialCatalog = info.DatabaseName,
            MultipleActiveResultSets = true
        };

        if (info.UseWindowsAuthentication)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = info.Username;
            builder.Password = info.Password;
            builder.TrustServerCertificate = true; // For development
        }

        return builder.ConnectionString;
    }

    public async Task SaveConnectionStringAsync(string connectionString)
    {
        // Update Web appsettings.json
        await UpdateAppSettingsAsync(_webAppSettingsPath, connectionString);
        
        // Update Web appsettings.Development.json
        await UpdateAppSettingsAsync(_webAppSettingsDevPath, connectionString);
        
        // Update API appsettings.json
        await UpdateAppSettingsAsync(_apiAppSettingsPath, connectionString);
        
        // Update API appsettings.Development.json
        await UpdateAppSettingsAsync(_apiAppSettingsDevPath, connectionString);
    }

    private async Task UpdateAppSettingsAsync(string filePath, string connectionString)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        var json = await File.ReadAllTextAsync(filePath);
        var jsonDocument = JsonDocument.Parse(json);
        var root = jsonDocument.RootElement;

        var settings = new Dictionary<string, object>();
        
        foreach (var property in root.EnumerateObject())
        {
            if (property.Name == "ConnectionStrings")
            {
                settings[property.Name] = new Dictionary<string, string>
                {
                    ["DefaultConnection"] = connectionString
                };
            }
            else
            {
                settings[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText())!;
            }
        }

        // Mark setup as complete
        settings["SetupComplete"] = true;

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var updatedJson = JsonSerializer.Serialize(settings, options);
        await File.WriteAllTextAsync(filePath, updatedJson);
    }
}

public class DatabaseConnectionInfo
{
    public string ServerName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "SimpleAccounts";
    public bool UseWindowsAuthentication { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
