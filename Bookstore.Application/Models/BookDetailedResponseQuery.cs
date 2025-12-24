namespace Bookstore.Application.Models
{
    internal record BookDetailedResponseQuery(
        int Id,
        string Title,
        float Price,
        string AuthorNames,
        string GenreNames,
        double AverageRating);
}
