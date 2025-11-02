namespace Bookstore.Application.Models
{
    internal record BookDetailedResponseQuery(
        int Id,
        string Title,
        string AuthorNames,
        string GenreNames,
        double AverageRating);
}
