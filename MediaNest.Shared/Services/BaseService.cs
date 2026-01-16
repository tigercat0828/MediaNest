using Microsoft.AspNetCore.Components.Authorization;

namespace MediaNest.Shared.Services;
public abstract class BaseService(AuthenticationStateProvider authStateProvider)
{
    protected async Task<string?> GetCurrentUsernameAsync()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity is not { IsAuthenticated: true })
        {
            return null;
        }
        return user.Identity.Name!;
    }

    protected async Task<bool> IsInRoleAsync(string role)
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        return authState.User.IsInRole(role);
    }

    protected async Task<bool> AuthorizeAsync(UserRole requiredRole)
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity is not { IsAuthenticated: true })
        {
            return false;
        }

        var currentRole = Roles.ToEnum(user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty);

        if (currentRole < requiredRole)
        {
            return false;
        }

        return true;
    }

    [Obsolete("Use AuthorizeAsync(UserRole) instead")]
    protected async Task<bool> EnsureRoleAsync(string role) => await AuthorizeAsync(Roles.ToEnum(role));
}