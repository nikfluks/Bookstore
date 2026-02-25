namespace Bookstore.Application.Models;

public record BookAuthorsUpdateRequest(IReadOnlyList<int> AuthorIds);
