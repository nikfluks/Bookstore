using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Models.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string AccessToken { get; set; } = string.Empty;
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
