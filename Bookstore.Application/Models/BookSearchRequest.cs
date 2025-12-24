namespace Bookstore.Application.Models
{
    public record BookSearchRequest(
        string? BookTitle = null,
        string? AuthorName = null,
        string? GenreName = null,
        float? MinPrice = null,
        float? MaxPrice = null,
        float? MinAverageRating = null
    );
}
