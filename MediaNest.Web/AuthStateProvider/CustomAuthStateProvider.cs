using MediaNest.Shared.Dtos;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MediaNest.Web.AuthStateProvider;
public class CustomAuthStateProvider(ProtectedLocalStorage localStorage) : AuthenticationStateProvider {
    public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
        
        var session = (await localStorage.GetAsync<AuthResponse>("sessionState")).Value;
        var identity = session == null ? new ClaimsIdentity() : GetClaimsIdentity(session.Token);
        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }

    public async Task MarkUserAsAuthenticated(AuthResponse response) { 
        
        await localStorage.SetAsync("sessionState", response);
        var identity = GetClaimsIdentity(response.Token);
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
    public async Task MarkUserAsLoggedOut() {
        
        await localStorage.DeleteAsync("sessionState");    // TODO : authToken
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

    }

    private ClaimsIdentity GetClaimsIdentity(string token) {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var claims = jwtToken.Claims;
        return new ClaimsIdentity(claims, "jwt");
    }
}
