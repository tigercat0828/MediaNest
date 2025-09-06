
using MediaNest.Shared.Dtos;
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
    }

    public static async Task<IResult> Login(
        AuthRequest request,
        IConfiguration configuration) {
     
        // use fake data for now
        if (request.Username == "admin" && request.Password == "admin") {
            var token = GenerateJwtToken(configuration, request.Username);
            return TypedResults.Ok(
                new AuthResponse { 
                    Token = token,
                    Expiration = DateTime.Now.AddHours(1)
                });
        }
        return TypedResults.Unauthorized();
    }

    private static string GenerateJwtToken(
        IConfiguration configuration,
        string username) {
        List<Claim> claims = [
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        ];
        string secret = configuration["Jwt:Key"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
