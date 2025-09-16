using MediaNest.Shared.Dtos;
using MediaNest.Shared.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MediaNest.ApiService.Services;

// TODO : delete account
// TODO : change password
// TODO : get user by DTO
// TODO : refresh token store with DB 
public class AuthService (IConfiguration configuration, IMongoCollection<Account> accounts){
    public async Task<List<Account>> GetAllUsersAsync() => await accounts.Find(_ => true).ToListAsync();
    public async Task<AuthResponse?> LoginAsync(AuthRequest request) {
        var user = await accounts.Find(u => u.Username == request.Username).FirstOrDefaultAsync();
        if (user is null) return null;

        var res = new PasswordHasher<Account>().VerifyHashedPassword(user, user.HashedPassword, request.Password);
        if (res == PasswordVerificationResult.Failed) return null;

        if (user.Role == "Pending")
            return new AuthResponse { Message = "Account not approved yet." };

        return new AuthResponse {
            Token = GenerateJwtToken(user, isRefreshToken: false),
            RefreshToken = GenerateJwtToken(user, isRefreshToken: true),
            Expiration = DateTime.UtcNow.AddHours(1),
            Message = "Login successful!"
        };
    }
    public async Task<AuthResponse?> RegisterAsync(AuthRequest request) {
        var exists = await accounts.Find(u => u.Username == request.Username).FirstOrDefaultAsync();
        if (exists != null) return null;

        var user = new Account {
            Username = request.Username,
            HashedPassword = new PasswordHasher<Account>().HashPassword(null!, request.Password),
            Role = "Pending"
        };

        await accounts.InsertOneAsync(user);

        return new AuthResponse {
            Token = GenerateJwtToken(user, isRefreshToken: false),
            RefreshToken = GenerateJwtToken(user, isRefreshToken: true),
            Expiration = DateTime.UtcNow.AddHours(1)
        };
    }
    public async Task<AuthResponse> ChangePassword(AuthRequest request) {
        var user = await accounts.Find(u => u.Username == request.Username).FirstOrDefaultAsync();
        if (user == null) return new AuthResponse { Message = "User not found " };

        var hasher = new PasswordHasher<Account>();
        var verifyResult = hasher.VerifyHashedPassword(user, user.HashedPassword, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed) return new AuthResponse { Message = "Password is incorrect" };

        if (string.IsNullOrEmpty(request.Password)) return new AuthResponse { Message = "New password is required" };

        var newHashedPassword = hasher.HashPassword(user, request.Password);
        var update = Builders<Account>.Update.Set(u => u.HashedPassword, newHashedPassword);
        var result = await accounts.UpdateOneAsync(u=>u.Username == request.Username, update);
        if (result.ModifiedCount == 0) return new AuthResponse { Message = "Password update failed." };

        return new AuthResponse {
            Token = GenerateJwtToken(user, false),
            RefreshToken = GenerateJwtToken(user, true),
            Expiration = DateTime.UtcNow.AddHours(1),
            Message = "Password changed successfully."
        };
    }
    public async Task<AuthResponse> DeleteAsync(string username) {
        // delete user by given username
        if (username == "admin") return new AuthResponse { Message = "Admin cannot be deleted" };

        var user = await accounts.Find(u => u.Username == username).FirstOrDefaultAsync();
        if (user == null) return new AuthResponse { Message = "User not found" };

        var result = await accounts.DeleteOneAsync(u => u.Username == username);
        if (result.DeletedCount == 0) return new AuthResponse { Message = "Delete failed" };
        return new AuthResponse { Message = "Account deleted successfully" };
    }
    public async Task<bool> UpdateUserRoleAsync(AccountUpdateRequest request) {
        var update = Builders<Account>.Update.Set(u => u.Role, request.Role);
        var result = await accounts.UpdateOneAsync(u => u.Username == request.Username, update);
        return result.ModifiedCount > 0;
    }
    public async Task SetSeedAdminIfNotExist() {
        var exist = await accounts.Find(u => u.Username == "admin").FirstOrDefaultAsync();
        if (exist != null) return;
        var request = new AuthRequest {
            Username = "admin",
            Password = "admin"
        };
        var user = await RegisterAsync(request);
        if (user != null) {
            // 註冊時預設 Role = "Pending"，所以這裡要直接升級成 Admin
            var update = Builders<Account>.Update.Set(u => u.Role, "Admin");
            await accounts.UpdateOneAsync(u => u.Username == request.Username, update);
        }
    }
    // Helper
    private string GenerateJwtToken(Account user, bool isRefreshToken) {

        List<Claim> claims = [
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        ];
        string secret = configuration[isRefreshToken ? "Jwt:RefreshKey" : "Jwt:Key"];
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
}
