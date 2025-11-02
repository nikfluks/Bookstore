using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Models
{
    public record ReviewUpdateRequest(
        int Id,
        string? Description,
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        int Rating);
}
