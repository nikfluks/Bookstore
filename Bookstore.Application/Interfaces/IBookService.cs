using Bookstore.Application.Models;
using Bookstore.Domain.Entities;

namespace Bookstore.Application.Interfaces;

public interface IBookService
{
    Task<PagedResponse<BookResponse>> GetAllAsync(PagedRequest request);
    Task<PagedResponse<BookDetailedResponse>> GetAllDetailedAsync(PagedRequest request);
    Task<IEnumerable<BookDetailedResponse>> GetTop10ByRatingAsync();
    Task<BookResponse?> GetByIdAsync(int id);
    Task<PagedResponse<BookDetailedResponse>> SearchBooksAsync(BookSearchRequest request);
    Task<BookDetailedResponse> CreateAsync(BookCreateRequest bookCreate);
    Task<BookResponse?> UpdateAsync(int id, BookPriceUpdateRequest priceUpdate);
    Task<BookDetailedResponse?> UpdateAuthorsAsync(int id, BookAuthorsUpdateRequest authorsUpdate);
    Task<BookDetailedResponse?> UpdateGenresAsync(int id, BookGenresUpdateRequest genresUpdate);
    Task<bool> DeleteAsync(int id);

    IQueryable<Book> GetBooksQueryable();
}
