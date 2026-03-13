using Asp.Versioning;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers;

[ApiVersion(1.0)]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest request)
    {
        var result = await authService.AuthenticateAsync(request);

        if (!result.IsAuthenticated)
        {
            return Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized");
        }

        return Ok(new LoginResponse
        {
            Token = result.Token!,
            Role = result.Role!,
            ExpiresAtUtc = result.ExpiresAtUtc!.Value
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);

        if (!result.IsSuccessful)
        {
            return Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request");
        }

        return Created();
    }
}
