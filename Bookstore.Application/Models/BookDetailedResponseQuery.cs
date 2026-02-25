namespace Bookstore.Application.Models;

public record BookDetailedResponseQuery
(
    int Id,
    string Title,
    float Price,
    string AuthorNames,
    string GenreNames,
    double AverageRating,
    int TotalCount
);
