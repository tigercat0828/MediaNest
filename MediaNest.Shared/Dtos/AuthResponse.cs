namespace MediaNest.Shared.Dtos;
public class AuthResponse {
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
    public string Message { get; set; }
}
