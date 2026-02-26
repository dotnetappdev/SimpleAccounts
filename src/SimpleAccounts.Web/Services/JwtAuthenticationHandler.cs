using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

namespace SimpleAccounts.Web.Services;

public class JwtAuthenticationHandler : DelegatingHandler
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public JwtAuthenticationHandler(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var customAuthProvider = (CustomAuthenticationStateProvider)_authStateProvider;
        var token = await customAuthProvider.GetTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
