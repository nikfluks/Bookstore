namespace Bookstore.Application.Models
{
    public record BookDetailedResponse(
        int Id,
        string Title,
        List<string> AuthorNames,
        List<string> GenreNames,
        double AverageRating);
}
