using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SimpleAccounts.Web.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(IHttpClientFactory httpClientFactory, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClientFactory.CreateClient("SimpleAccountsAPI");
        _authStateProvider = authStateProvider;
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
            {
                Email = email,
                Password = password
            });

            if (!response.IsSuccessStatusCode)
            {
                return new LoginResult { Success = false, Message = "Invalid email or password" };
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginResponse == null)
            {
                return new LoginResult { Success = false, Message = "Invalid response from server" };
            }

            // Store token and user info
            await ((CustomAuthenticationStateProvider)_authStateProvider).MarkUserAsAuthenticated(loginResponse);

            return new LoginResult { Success = true };
        }
        catch (Exception ex)
        {
            return new LoginResult { Success = false, Message = ex.Message };
        }
    }

    public async Task LogoutAsync()
    {
        await ((CustomAuthenticationStateProvider)_authStateProvider).MarkUserAsLoggedOut();
    }

    public async Task<string?> GetTokenAsync()
    {
        return await ((CustomAuthenticationStateProvider)_authStateProvider).GetTokenAsync();
    }
}

public class LoginResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime ExpiresAt { get; set; }
}
