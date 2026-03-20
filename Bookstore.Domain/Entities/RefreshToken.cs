using Microsoft.AspNetCore.Identity;

namespace Bookstore.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public required string Token { get; set; }
    public required string JwtId { get; set; }
    public required string UserId { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }

    public bool IsRevoked => RevokedAtUtc is not null;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsActive => !IsRevoked && !IsExpired;

    public IdentityUser User { get; set; } = null!;
}
