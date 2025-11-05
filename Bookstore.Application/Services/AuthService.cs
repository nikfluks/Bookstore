using Bookstore.Application.Constants;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Models.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Bookstore.Application.Services
{
    internal class AuthService(IOptions<JwtSettings> jwtSettingsOption, IConfiguration configuration) : IAuthService
    {
        private readonly JwtSettings jwtSettings = jwtSettingsOption.Value;

        public Task<AuthenticationResult> AuthenticateAsync(LoginRequest request)
        {
            var role = ValidateUser(request.Username, request.Password);

            if (role == null)
            {
                return Task.FromResult(AuthenticationResult.Failure("Invalid username or password"));
            }

            var token = GenerateJwtToken(request.Username, role);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes);

            return Task.FromResult(AuthenticationResult.Success(token, role, expiresAtUtc));
        }

        private string? ValidateUser(string username, string password)
        {
            var readUsername = configuration["UsersAuth:ReadUser:Username"];
            var readPasswordHash = configuration["UsersAuth:ReadUser:PasswordHash"];
            var adminUsername = configuration["UsersAuth:AdminUser:Username"];
            var adminPasswordHash = configuration["UsersAuth:AdminUser:PasswordHash"];

            var passwordHash = ComputeSha256Hash(password);

            if (username == readUsername && passwordHash == readPasswordHash)
            {
                return Roles.Read;
            }

            if (username == adminUsername && passwordHash == adminPasswordHash)
            {
                return Roles.ReadWrite;
            }

            return null;
        }

        private static string ComputeSha256Hash(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        private string GenerateJwtToken(string username, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

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
}
