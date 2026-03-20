using Bookstore.Application.Models.Auth;

namespace Bookstore.Application.Interfaces;

public interface IAuthService
{
    Task<AuthenticationResult> LoginAsync(LoginRequest request);
    Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest request);
    Task<RegisterResult> RegisterAsync(RegisterRequest request);
}
