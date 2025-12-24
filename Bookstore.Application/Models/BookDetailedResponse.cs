namespace Bookstore.Application.Models
{
    public record BookDetailedResponse(
        int Id,
        string Title,
        float Price,
        List<string> AuthorNames,
        List<string> GenreNames,
        double AverageRating);
}
