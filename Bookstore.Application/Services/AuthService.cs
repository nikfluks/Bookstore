using Bookstore.Application.Constants;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Bookstore.Application.Services;

internal class AuthService(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IOptions<JwtSettings> jwtSettingsOption) : IAuthService
{
    private readonly JwtSettings jwtSettings = jwtSettingsOption.Value;

    public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Username);

        if (user is null)
        {
            return AuthenticationResult.Failure("Invalid username or password");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return AuthenticationResult.Failure("Invalid username or password");
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user.UserName!, roles);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes);

        return AuthenticationResult.Success(token, string.Join(",", roles), expiresAtUtc);
    }

    /// <summary>
    /// By default, assign the "Read" role to new users. Adjust as needed.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        var user = new IdentityUser { UserName = request.Username };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return RegisterResult.Failure(errors);
        }

        await userManager.AddToRoleAsync(user, Roles.Read);

        return RegisterResult.Success();
    }

    private string GenerateJwtToken(string username, IList<string> roles)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken
        (
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
