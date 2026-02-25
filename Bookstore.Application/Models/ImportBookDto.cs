namespace Bookstore.Application.Models;

public record ImportBookDto
(
    string Title,
    float Price,
    IReadOnlyList<string> AuthorNames,
    IReadOnlyList<string> GenreNames
);
