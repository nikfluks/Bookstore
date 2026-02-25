namespace Bookstore.Application.Models;

public record BookCreateRequest
(
    string Title,
    float Price,
    IReadOnlyList<int>? AuthorIds = null,
    IReadOnlyList<int>? GenreIds = null
);
