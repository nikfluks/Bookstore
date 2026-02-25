namespace Bookstore.Application.Models;

public record BookDetailedResponse
(
    int Id,
    string Title,
    float Price,
    IReadOnlyList<string> AuthorNames,
    IReadOnlyList<string> GenreNames,
    double AverageRating
);
