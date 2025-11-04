using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Models
{
    public record ReviewUpdateRequest(
        string? Description,
        [Range(1, 5)]
        int Rating);
}
