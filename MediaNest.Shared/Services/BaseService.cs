using Microsoft.AspNetCore.Components.Authorization;

namespace MediaNest.Shared.Services;
public abstract class BaseService(AuthenticationStateProvider authStateProvider)
{
    protected async Task<string> GetCurrentUsernameAsync()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity is not { IsAuthenticated: true })
        {
            throw new UnauthorizedAccessException("操作失敗：使用者未登入");
        }
        return user.Identity.Name!;
    }

    protected async Task<bool> IsInRoleAsync(string role)
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        return authState.User.IsInRole(role);
    }

    protected async Task EnsureRoleAsync(string role)
    {
        if (!await IsInRoleAsync(role))
        {
            throw new UnauthorizedAccessException($"權限不足：需要 {role} 角色");
        }
    }
}