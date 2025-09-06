
using MediaNest.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Validations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MediaNest.ApiService.Endpoints; 
public static class AuthEndpoints {
    public static void MapAuthEndpoints(this IEndpointRouteBuilder builder) { 
        var group = builder.MapGroup("/api/account").WithTags("Account");
        // group.MapPost("/register", RegisterAccount);
        group.MapPost("/login", Login);
        group.MapGet("/refreshLogin", RefreshLogin);
    }

    public static async Task<IResult> Login(
        IConfiguration configuration,
        AuthRequest request
        ) {

        // TODO : use fake data for now
        if (request.Username == "admin" && request.Password == "admin" ||
            request.Username == "user" && request.Password == "user"

            ) {
            var token = GenerateJwtToken(configuration, request.Username, isRefreshToken: false);
            var refreshToken = GenerateJwtToken(configuration, request.Password, isRefreshToken: true);
            return TypedResults.Ok(new AuthResponse {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddHours(1)
            });
        }
        return TypedResults.Unauthorized();
    }

    private static string GenerateJwtToken(
        IConfiguration configuration,
        string username,
        bool isRefreshToken
        ) {

        List<Claim> claims = [
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, username == "admin" ? "Admin": "User")
        ];
        string secret = configuration[isRefreshToken ? "Jwt:RefreshKey" : "Jwt:Key" ];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(isRefreshToken ? 24 : 1),
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
        var newToken = GenerateJwtToken(configuration, username, isRefreshToken: false);
        var newRefreshToken = GenerateJwtToken(configuration, username, isRefreshToken: true);

        return TypedResults.Ok(new AuthResponse {
            Token = newToken,
            RefreshToken = newRefreshToken,
            Expiration = DateTime.UtcNow.AddHours(1)
        });
    }
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
}
