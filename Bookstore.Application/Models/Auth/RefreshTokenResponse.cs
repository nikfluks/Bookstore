namespace Bookstore.Application.Models.Auth;

public class RefreshTokenResponse
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required string Role { get; set; }
    public required DateTime AccessTokenExpiresAtUtc { get; set; }
    public required DateTime RefreshTokenExpiresAtUtc { get; set; }
}
