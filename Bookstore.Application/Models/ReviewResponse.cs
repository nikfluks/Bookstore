namespace Bookstore.Application.Models
{
    public record ReviewResponse(int Id, string? Description, int Rating, string BookTitle);
}
