using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Models.Auth;

public class RegisterRequest
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
