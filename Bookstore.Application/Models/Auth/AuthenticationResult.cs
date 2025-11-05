namespace Bookstore.Application.Models.Auth
{
    public class AuthenticationResult
    {
        public bool IsAuthenticated { get; set; }
        public string? Token { get; set; }
        public string? Role { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string? ErrorMessage { get; set; }

        public static AuthenticationResult Success(string token, string role, DateTime expiresAtUtc)
        {
            return new AuthenticationResult
            {
                IsAuthenticated = true,
                Token = token,
                Role = role,
                ExpiresAtUtc = expiresAtUtc
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
}
