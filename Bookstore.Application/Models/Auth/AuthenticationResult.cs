namespace Bookstore.Application.Models.Auth;

public class AuthenticationResult
{
    public bool IsAuthenticated { get; set; }
    public string? AccessToken { get; set; }
    public string? Role { get; set; }
    public DateTime? AccessTokenExpiresAtUtc { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }
    public string? ErrorMessage { get; set; }

    public static AuthenticationResult Success
    (
        string accessToken,
        string role,
        DateTime accessTokenExpiresAtUtc,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc
    )
    {
        return new AuthenticationResult
        {
            IsAuthenticated = true,
            AccessToken = accessToken,
            Role = role,
            AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc
        };
    }

    public static AuthenticationResult Failure(string errorMessage)
    {
        return new AuthenticationResult
        {
            IsAuthenticated = false,
            ErrorMessage = errorMessage
        };
    }
}
