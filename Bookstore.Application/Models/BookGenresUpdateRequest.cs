namespace Bookstore.Application.Models;

public record BookGenresUpdateRequest(IReadOnlyList<int> GenreIds);
