namespace Bookstore.Application.Models
{
    public record BookSearchRequest(
        string? SearchTerm = null,
        string? AuthorName = null,
        string? GenreName = null,
        float? MinPrice = null,
        float? MaxPrice = null,
        float? MinRating = null
    );
}
