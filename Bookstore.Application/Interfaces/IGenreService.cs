using Bookstore.Application.Models;

namespace Bookstore.Application.Interfaces
{
    public interface IGenreService
    {
        Task<IEnumerable<GenreResponse>> GetAllAsync();
        Task<GenreResponse?> GetByIdAsync(int id);
        Task<GenreResponse> CreateAsync(GenreCreateRequest genreCreate);
        Task<GenreResponse?> UpdateAsync(GenreUpdateRequest genreUpdate);
        Task<bool> DeleteAsync(int id);
    }
}
