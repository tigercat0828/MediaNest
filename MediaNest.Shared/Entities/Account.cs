
namespace MediaNest.Shared.Entities;
public class Account {
    public int Id { get; set; }
    public string Username { get; set; }
    public string HashedPassword { get; set; }
    public string Role { get; set; } = "User";
}
