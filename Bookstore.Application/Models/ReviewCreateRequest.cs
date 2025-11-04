using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Models
{
    public record ReviewCreateRequest(
        string? Description,
        [Range(1, 5)]
        int Rating,
        int BookId);
}
