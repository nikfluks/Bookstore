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
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);

        if (!result.IsAuthenticated)
        {
            return Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized");
        }

        return Ok(new LoginResponse
        {
            AccessToken = result.AccessToken!,
            Role = result.Role!,
            AccessTokenExpiresAtUtc = result.AccessTokenExpiresAtUtc!.Value,
            RefreshToken = result.RefreshToken!,
            RefreshTokenExpiresAtUtc = result.RefreshTokenExpiresAtUtc!.Value
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
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

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await authService.RefreshTokenAsync(request);

        if (!result.IsAuthenticated)
        {
            return Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request");
        }

        var response = new RefreshTokenResponse
        {
            AccessToken = result.AccessToken!,
            Role = result.Role!,
            AccessTokenExpiresAtUtc = result.AccessTokenExpiresAtUtc!.Value,
            RefreshToken = result.RefreshToken!,
            RefreshTokenExpiresAtUtc = result.RefreshTokenExpiresAtUtc!.Value
        };

        return Ok(response);
    }
}
