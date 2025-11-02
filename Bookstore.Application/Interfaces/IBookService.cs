using Bookstore.Application.Models;

namespace Bookstore.Application.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookResponse>> GetAllAsync();
        Task<BookResponse?> GetByIdAsync(int id);
        Task<BookResponse> CreateAsync(BookCreateRequest bookCreate);
        Task<BookResponse?> UpdateAsync(BookUpdateRequest bookUpdate);
        Task<bool> DeleteAsync(int id);
    }
}
