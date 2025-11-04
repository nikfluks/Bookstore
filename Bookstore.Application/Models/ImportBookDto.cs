namespace Bookstore.Application.Models
{
    public record ImportBookDto(
        string Title,
        float Price,
        List<string> AuthorNames,
        List<string> GenreNames);
}
