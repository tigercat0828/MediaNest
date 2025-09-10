
using MediaNest.Shared.Dtos;
using MediaNest.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Validations;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace MediaNest.Web.Endpoints; 
public static class AuthServiceEndpoints {
    // TODO : refresh token store with DB 
    // TODO : change password
    // TODO : extract to AuthService
    public static void MapAuthEndpoints(this IEndpointRouteBuilder builder) { 
        var group = builder.MapGroup("/api/account").WithTags("Account");
        group.MapPost("/login", Login);
        group.MapPost("/register", Register);
        group.MapGet("/refreshLogin", RefreshLogin);
        group.MapGet("/users", GetAllUsers).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        group.MapPut("/users/updateRole", ToggleUserRole).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }

    public static async Task<IResult> GetAllUsers(IMongoCollection<Account> accounts) {
        var users = await accounts.Find(_ => true).ToListAsync();
        return Results.Ok(users);
    }
    public static async Task<IResult> Login(
        IConfiguration configuration,
        IMongoCollection<Account> accounts,
        AuthRequest request
        ) {
        
        var user = await accounts.Find(u => u.Username == request.Username).FirstOrDefaultAsync();
        if(user is null) {
            return Results.Unauthorized();
        }
        var res = new PasswordHasher<Account>().VerifyHashedPassword(user, user.HashedPassword, request.Password);
        if (res == PasswordVerificationResult.Failed) {
            return Results.Unauthorized();
        }

        if (user.Role == "Pending") {
            return TypedResults.BadRequest(new AuthResponse {
                Message = "Account not approved yet."
            });
        }

        var token = GenerateJwtToken(configuration, user, isRefreshToken: false);
        var refreshToken = GenerateJwtToken(configuration, user, isRefreshToken: true);
        return Results.Ok(new AuthResponse {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddHours(1),
            Message = "Login successful!"
        });
    }

    private static string GenerateJwtToken(
        IConfiguration configuration,
        Account user,
        bool isRefreshToken
        ) {

        List<Claim> claims = [
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        ];
        string secret = configuration[isRefreshToken ? "Jwt:RefreshKey" : "Jwt:Key" ];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(isRefreshToken ? 24 : 1),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static IResult RefreshLogin(IConfiguration configuration, string refreshToken) {
        var secret = configuration["Jwt:RefreshKey"];
        var claimsPrincipal = GetClaimsPrincipalFromToken(refreshToken, secret);
        if (claimsPrincipal == null) {
            return TypedResults.Unauthorized();
        }

        var username = claimsPrincipal.FindFirstValue(ClaimTypes.Name);
        var role = claimsPrincipal.FindFirstValue(ClaimTypes.Role);
        var fakeUser = new Account {
            Username = username,
            Role = role
        };
        var newToken = GenerateJwtToken(configuration, fakeUser, isRefreshToken: false);
        var newRefreshToken = GenerateJwtToken(configuration, fakeUser, isRefreshToken: true);

        return TypedResults.Ok(new AuthResponse {
            Token = newToken,
            RefreshToken = newRefreshToken,
            Expiration = DateTime.UtcNow.AddHours(1)
        });
    }
    private static async Task<IResult> Register(
        IConfiguration configuration,
        IMongoCollection<Account> accounts,
        AuthRequest request) 
    {
        var exists = await accounts.Find(u => u.Username == request.Username).FirstOrDefaultAsync();
        if (exists != null) {
            return TypedResults.BadRequest(new AuthResponse {
                Message = "Username exists"
            });
        }
        var user = new Account();
        var hashedPassed = new PasswordHasher<Account>().HashPassword(user, request.Password);
        user.Username = request.Username;
        user.HashedPassword = hashedPassed;
        user.Role = "Pending";
        await accounts.InsertOneAsync(user);

        string token = GenerateJwtToken(configuration, user, isRefreshToken: false);
        string refreshToken = GenerateJwtToken(configuration, user, isRefreshToken: true);

        return TypedResults.Ok(new AuthResponse { 
            Token = token,
            RefreshToken = refreshToken,
            Expiration= DateTime.UtcNow.AddHours(1)
        });
    }
    /// <summary>
    /// Who am I ? 
    /// </summary>
    private static ClaimsPrincipal GetClaimsPrincipalFromToken(string token, string secret) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret); // ASCII ? 

        try {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters {
                ValidateAudience = true,
                ValidAudience = "MediaNestUser",
                ValidateIssuer = true,
                ValidIssuer = "MediaNestApi",
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out var validatedToken);
            return principal; 
        }
        catch {
            return null;
        }

    }
    private static async Task<IResult> ToggleUserRole(IMongoCollection<Account> accounts, AccountUpdateRequest request ) {
        var update = Builders<Account>.Update.Set(u => u.Role, request.Role);
        await accounts.UpdateOneAsync(u => u.Username == request.Username, update);
        return Results.Ok("success");
    }
}
