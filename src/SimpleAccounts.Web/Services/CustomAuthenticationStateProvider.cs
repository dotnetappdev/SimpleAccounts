using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SimpleAccounts.Web.Services;

namespace SimpleAccounts.Web.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var tokenResult = await _sessionStorage.GetAsync<string>("authToken");
            if (!tokenResult.Success || string.IsNullOrWhiteSpace(tokenResult.Value))
            {
                return new AuthenticationState(_anonymous);
            }

            var claims = ParseClaimsFromJwt(tokenResult.Value);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task MarkUserAsAuthenticated(LoginResponse loginResponse)
    {
        await _sessionStorage.SetAsync("authToken", loginResponse.Token);
        await _sessionStorage.SetAsync("userEmail", loginResponse.Email);
        await _sessionStorage.SetAsync("userFirstName", loginResponse.FirstName);
        await _sessionStorage.SetAsync("userLastName", loginResponse.LastName);
        await _sessionStorage.SetAsync("userRoles", loginResponse.Roles);
        await _sessionStorage.SetAsync("tokenExpiresAt", loginResponse.ExpiresAt);

        var claims = ParseClaimsFromJwt(loginResponse.Token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _sessionStorage.DeleteAsync("authToken");
        await _sessionStorage.DeleteAsync("userEmail");
        await _sessionStorage.DeleteAsync("userFirstName");
        await _sessionStorage.DeleteAsync("userLastName");
        await _sessionStorage.DeleteAsync("userRoles");
        await _sessionStorage.DeleteAsync("tokenExpiresAt");

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var tokenResult = await _sessionStorage.GetAsync<string>("authToken");
            return tokenResult.Success ? tokenResult.Value : null;
        }
        catch
        {
            return null;
        }
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        return token.Claims;
    }
}

