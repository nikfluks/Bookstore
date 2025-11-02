namespace Bookstore.Application.Models
{
    public record AuthorUpdateRequest(int Id, string Name, int BirthYear);
}
