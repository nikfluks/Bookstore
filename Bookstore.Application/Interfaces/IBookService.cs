using Bookstore.Application.Models;

namespace Bookstore.Application.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookResponse>> GetAllAsync();
        Task<IEnumerable<BookDetailedResponse>> GetAllDetailedAsync();
        Task<IEnumerable<BookDetailedResponse>> GetTop10ByRatingAsync();
        Task<BookResponse?> GetByIdAsync(int id);
        Task<IEnumerable<BookDetailedResponse>> SearchBooksAsync(BookSearchRequest request);
        Task<BookDetailedResponse> CreateAsync(BookCreateRequest bookCreate);
        Task<BookResponse?> UpdateAsync(int id, BookPriceUpdateRequest priceUpdate);
        Task<BookDetailedResponse?> UpdateAuthorsAsync(int id, BookAuthorsUpdateRequest authorsUpdate);
        Task<BookDetailedResponse?> UpdateGenresAsync(int id, BookGenresUpdateRequest genresUpdate);
        Task<bool> DeleteAsync(int id);
    }
}
