using Bookstore.Application.Interfaces;
using Bookstore.Application.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await authService.AuthenticateAsync(request);

            if (!result.IsAuthenticated)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            return Ok(new LoginResponse
            {
                Token = result.Token!,
                Role = result.Role!,
                ExpiresAtUtc = result.ExpiresAtUtc!.Value
            });
        }
    }
}
