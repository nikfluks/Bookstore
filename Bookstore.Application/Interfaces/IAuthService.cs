using Bookstore.Application.Models.Auth;

namespace Bookstore.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthenticationResult> AuthenticateAsync(LoginRequest request);
    }
}
