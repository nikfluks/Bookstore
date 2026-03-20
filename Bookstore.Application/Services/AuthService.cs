using Bookstore.Application.Constants;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Models.Auth;
using Bookstore.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Bookstore.Application.Services;

internal class AuthService
(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IOptions<JwtSettings> jwtSettingsOption,
    TokenValidationParameters tokenValidationParameters,
    IAppDbContext db
) : IAuthService
{
    private readonly JwtSettings jwtSettings = jwtSettingsOption.Value;

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request)
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
        var (accessToken, expiresAtUtc) = GenerateAccessToken(user.UserName!, roles);

        var refreshToken = await GenerateRefreshTokenAsync(accessToken, user);

        return AuthenticationResult.Success
        (
            accessToken,
            string.Join(",", roles),
            expiresAtUtc,
            refreshToken.Token,
            refreshToken.ExpiresAtUtc
        );
    }

    /// <summary>
    /// By default, assign the "Read" role to new users.
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

    public async Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var accessTokenPrincipal = GetPrincipalFromAccessToken(request.AccessToken, tokenValidationParameters);
        if (accessTokenPrincipal is null)
        {
            return AuthenticationResult.Failure("Invalid access token");
        }

        var jti = accessTokenPrincipal.Claims
            .SingleOrDefault(x => string.Equals(x.Type, JwtRegisteredClaimNames.Jti, StringComparison.OrdinalIgnoreCase))?.Value;

        if (string.IsNullOrEmpty(jti))
        {
            return AuthenticationResult.Failure("Invalid access token");
        }

        var storedRefreshToken = await db.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (storedRefreshToken is null)
        {
            return AuthenticationResult.Failure("The refresh token does not exist");
        }

        if (storedRefreshToken.IsExpired)
        {
            return AuthenticationResult.Failure("The refresh token has expired");
        }

        if (storedRefreshToken.IsRevoked)
        {
            return AuthenticationResult.Failure("The refresh token has been revoked");
        }

        if (!string.Equals(storedRefreshToken.JwtId, jti, StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticationResult.Failure("The refresh token does not match the access token");
        }

        var username = accessTokenPrincipal.Claims.FirstOrDefault(x => string.Equals(x.Type, ClaimTypes.Name, StringComparison.OrdinalIgnoreCase))?.Value;
        if (username is null)
        {
            return AuthenticationResult.Failure("Current user is not found in access token");
        }

        var user = await userManager.FindByNameAsync(username);
        if (user is null)
        {
            return AuthenticationResult.Failure("Current user is not found in the database");
        }

        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiresAtUtc) = GenerateAccessToken(user.UserName!, roles);
        var refreshToken = await GenerateRefreshTokenAsync(accessToken, user, request.RefreshToken);

        return AuthenticationResult.Success
        (
            accessToken,
            string.Join(",", roles),
            expiresAtUtc,
            refreshToken.Token,
            refreshToken.ExpiresAtUtc
        );
    }

    /// <summary>
    /// Access token is JWT token.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="roles"></param>
    /// <returns>Access (JWT) token and its expiry date.</returns>
    private (string AccessToken, DateTime ExpiresAtUtc) GenerateAccessToken(string username, IList<string> roles)
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

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes);

        var accessToken = new JwtSecurityToken
        (
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(accessToken), expiresAtUtc);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5404:Do not disable token validation checks", Justification = "Ok for refresh token validation")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Ok for refresh token validation")]
    private static ClaimsPrincipal? GetPrincipalFromAccessToken(string accessToken, TokenValidationParameters parameters)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenParameters = parameters.Clone();
            tokenParameters.ValidateLifetime = false;
            var principal = tokenHandler.ValidateToken(accessToken, tokenParameters, out var validatedToken);
            return IsJwtWithValidSecurityAlgorithm(validatedToken) ? principal : null;
        }
        catch
        {
            return null;
        }
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        => validatedToken is JwtSecurityToken jwtSecurityToken
           && jwtSecurityToken.Header.Alg
                .Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase);

    private async Task<RefreshToken> GenerateRefreshTokenAsync
    (
        string accessToken,
        IdentityUser user,
        string? existingRefreshToken = null
    )
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(accessToken);
        var jti = jwtToken.Id;

        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
            JwtId = jti,
            UserId = user.Id,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationDays),
            CreatedAtUtc = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(existingRefreshToken))
        {
            await db.RefreshTokens
                .Where(x => x.Token == existingRefreshToken && x.RevokedAtUtc == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(token => token.RevokedAtUtc, _ => DateTime.UtcNow));
        }
        else
        {
            await db.RefreshTokens
                .Where(x => x.UserId == user.Id && x.RevokedAtUtc == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(token => token.RevokedAtUtc, _ => DateTime.UtcNow));
        }

        await db.RefreshTokens.AddAsync(refreshToken);
        await db.SaveChangesAsync();

        return refreshToken;
    }
}
