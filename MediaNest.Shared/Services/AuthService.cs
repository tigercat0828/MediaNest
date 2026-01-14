using MediaNest.Shared.Dtos;
using MediaNest.Shared.Entities;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System.Security.Claims;

namespace MediaNest.Shared.Services;

// TODO cache the user 
public class AuthService(IMongoCollection<Account> _accounts) {
    private readonly PasswordHasher<Account> _passwordHasher = new();
    public async Task<List<AccountDto>> GetAllUserAsync() {
        var users = await _accounts
            .Find(_ => true)
            .Project(u => new AccountDto {
                Username = u.Username,
                Role = u.Role
            }).ToListAsync();

        return users;
    }
    public async Task<AuthResponse> LoginAsync(string username, string password) {
        AuthResponse response = new();
        var user = await _accounts.Find(a => a.Username == username).FirstOrDefaultAsync();
        if (user == null) {
            response.Message = "invalid username or password.";
            return response;
        }
        var verify = _passwordHasher.VerifyHashedPassword(user, user.HashedPassword, password);
        if (verify == PasswordVerificationResult.Failed) {
            response.Message = "invalid username or password.";
            return response;
        }

        response.Principal = MakePrincipal(user);
        response.Message = "success";
        return response;
    }
    public async Task<AuthResponse> RegisterAsync(string username, string password) {
        AuthResponse response = new();
        var existingUser = await _accounts.Find(a => a.Username == username).FirstOrDefaultAsync();
        if (existingUser != null) {
            response.Message = "username already exists.";
            return response;
        }
        var newUser = new Account() {
            Username = username,
            HashedPassword = _passwordHasher.HashPassword(null, password),
            Role = Roles.Pending
        };

        await _accounts.InsertOneAsync(newUser);
        response.Message = "registration successful.";
        response.Principal = MakePrincipal(newUser);
        return response;
    }
    public async Task<AuthResponse> DeleteAsync(string username) {
        var response = new AuthResponse();
        var user = await _accounts.Find(u => u.Username == username).FirstOrDefaultAsync();
        if (user.Role == Roles.Admin) {
            response.Message = "can't delete admins.";
            return response;
        }
        var result = await _accounts.DeleteOneAsync(u => u.Username == username);
        response.Count = (int)result.DeletedCount;
        return response;
    }
    public async Task<AuthResponse> UpdateRoleAsync(string username, string role) {
        var update = Builders<Account>.Update.Set(u => u.Role, role);
        var result = await _accounts.UpdateOneAsync(u => u.Username == username, update);
        return new AuthResponse {
            Count = (int)result.ModifiedCount,
            Message = "success"
        };
    }

    public async Task<AuthResponse> ChangePassword(string username, string oldPassword, string newPassword) {
        var user = await _accounts.Find(u => u.Username == username).FirstOrDefaultAsync();

        var verify = _passwordHasher.VerifyHashedPassword(user, user.HashedPassword, oldPassword);
        if (verify == PasswordVerificationResult.Failed) {
            return new AuthResponse { Message = "wrong password." };
        }

        var hashedPassword = _passwordHasher.HashPassword(null, newPassword);
        var update = Builders<Account>.Update.Set(u => u.HashedPassword, hashedPassword);
        var result = await _accounts.UpdateOneAsync(u => u.Username == username, update);
        return new AuthResponse { Message = "success" };
    }
    public async Task CreateSeedAdmin() {
        var exist = await _accounts.Find(a => a.Username == "admin").FirstOrDefaultAsync();
        if (exist != null) return;
        var seedAdmin = new Account() {
            Username = "admin",
            HashedPassword = new PasswordHasher<Account>().HashPassword(null, "admin"),
            Role = Roles.Admin
        };
        await _accounts.InsertOneAsync(seedAdmin);
    }

    private ClaimsPrincipal MakePrincipal(Account user) {
        List<Claim> claims = [
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        ];
        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);
        return principal;
    }
}

public class AuthResponse {
    public bool Success => Principal != null;   // login/register success or not
    public string Message { get; set; }         // information
    public ClaimsPrincipal Principal { get; set; }
    public int Count { get; set; }      // modified count
}

public static class Roles {
    public const string Pending = "Pending";
    public const string Admin = "Admin";
    public const string User = "User";
}