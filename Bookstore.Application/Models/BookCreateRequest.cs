namespace Bookstore.Application.Models
{
    public record BookCreateRequest(
        string Title, 
        float Price, 
        List<int>? AuthorIds = null, 
        List<int>? GenreIds = null);
}
