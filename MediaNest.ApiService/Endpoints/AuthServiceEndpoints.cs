
using MediaNest.ApiService.Services;
using MediaNest.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace MediaNest.Web.Endpoints;

public static class AuthServiceEndpoints {

    public static void MapAuthEndpoints(this IEndpointRouteBuilder builder) {
        var group = builder.MapGroup("/api/account").WithTags("Account");
        group.MapPost("/login", Login);
        group.MapPost("/register", Register);
        group.MapGet("/users", GetAllUsers).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        group.MapPut("/users/updateRole", ToggleUserRole).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        group.MapPost("/changePassword", ChangePassword).RequireAuthorization();
        group.MapDelete("/delete/{username}", DeleteAccount).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }

    public static async Task<IResult> GetAllUsers(AuthService authService) {
        var users = await authService.GetAllUsersAsync();
        return Results.Ok(users);
    }
    public static async Task<IResult> Login(AuthService authService, AuthRequest request) {

        var response = await authService.LoginAsync(request);
        return response is null ? Results.Unauthorized() : Results.Ok(response);
    }

    private static async Task<IResult> Register(AuthService authService, AuthRequest request) {
        var response = await authService.RegisterAsync(request);
        return response is null
            ? Results.BadRequest(new AuthResponse { Message = "Username exists" })
            : Results.Ok(response);
    }

    private static async Task<IResult> ToggleUserRole(AuthService authService, AccountUpdateRequest request) {
        var success = await authService.UpdateUserRoleAsync(request);
        return success ? Results.Ok("success") : Results.NotFound("User not found");
    }

    // 新增：變更密碼 endpoint
    private static async Task<IResult> ChangePassword(AuthService authService, AccountUpdateRequest request) {
        var response = await authService.ChangePassword(request);
        if (!string.IsNullOrEmpty(response.Message) && response.Message.Contains("success", StringComparison.OrdinalIgnoreCase))
            return Results.Ok(response);
        return Results.BadRequest(response);
    }

    // 新增：刪除帳號 endpoint
    private static async Task<IResult> DeleteAccount(AuthService authService, string username) {
        var response = await authService.DeleteAsync(username);
        if (!string.IsNullOrEmpty(response.Message) && response.Message.Contains("success", StringComparison.OrdinalIgnoreCase))
            return Results.Ok(response);
        return Results.BadRequest(response);
    }
}
