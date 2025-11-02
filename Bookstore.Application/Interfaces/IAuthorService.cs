using Bookstore.Application.Models;

namespace Bookstore.Application.Interfaces
{
    public interface IAuthorService
    {
        Task<IEnumerable<AuthorResponse>> GetAllAsync();
        Task<AuthorResponse?> GetByIdAsync(int id);
        Task<AuthorResponse> CreateAsync(AuthorCreateRequest authorCreate);
        Task<AuthorResponse?> UpdateAsync(AuthorUpdateRequest authorUpdate);
        Task<bool> DeleteAsync(int id);
    }
}
