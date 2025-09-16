namespace MediaNest.Shared.Dtos;

public class AccountUpdateRequest() {
    public string Username { get; set; }
    public string Role { get; set; }
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }    
}