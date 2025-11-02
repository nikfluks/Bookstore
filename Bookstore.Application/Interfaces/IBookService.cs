using Bookstore.Application.Models;

namespace Bookstore.Application.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookDetailedResponse>> GetAllDetailedAsync();
        Task<IEnumerable<BookDetailedResponse>> GetTop10ByRatingAsync();
        Task<BookDetailedResponse?> GetByIdAsync(int id);
        Task<BookDetailedResponse> CreateAsync(BookCreateRequest bookCreate);
        Task<BookDetailedResponse?> UpdateAsync(int id, BookPriceUpdateRequest priceUpdate);
        Task<BookDetailedResponse?> UpdateAuthorsAsync(int id, BookAuthorsUpdateRequest authorsUpdate);
        Task<BookDetailedResponse?> UpdateGenresAsync(int id, BookGenresUpdateRequest genresUpdate);
        Task<bool> DeleteAsync(int id);
    }
}
