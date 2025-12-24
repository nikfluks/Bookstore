using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
    internal class BookService(IAppDbContext db, ILogger<BookService> logger) : IBookService
    {
        public async Task<IEnumerable<BookResponse>> GetAllAsync()
        {
            logger.LogInformation("Retrieving all books");

            try
            {
                var books = await db.Books
                    .Select(b => new BookResponse(
                        b.Id,
                        b.Title,
                        b.Price
                    ))
                    .ToListAsync();

                logger.LogInformation("Retrieved {BookCount} books", books.Count);
                return books;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all books");
                throw;
            }
        }

        public async Task<IEnumerable<BookDetailedResponse>> GetAllDetailedAsync()
        {
            logger.LogInformation("Retrieving all books");

            try
            {
                var books = await db.Books
                    .Select(b => new BookDetailedResponse(
                        b.Id,
                        b.Title,
                        b.Price,
                        b.Authors.Select(a => a.Name).ToList(),
                        b.Genres.Select(g => g.Name).ToList(),
                        b.Reviews.Count > 0 ? Math.Round(b.Reviews.Average(r => r.Rating), 2) : 0
                    ))
                    .ToListAsync();

                logger.LogInformation("Retrieved {BookCount} books", books.Count);
                return books;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all books");
                throw;
            }
        }

        public async Task<IEnumerable<BookDetailedResponse>> GetTop10ByRatingAsync()
        {
            logger.LogInformation("Retrieving top 10 books by rating");

            try
            {
                #region Raw query
                var sql = @"
                WITH BookRatings AS (
	                SELECT TOP 10
		                b.Id,
		                COALESCE(AVG(CAST(r.Rating AS FLOAT)), 0) AS AverageRating
	                FROM Books b
	                LEFT JOIN Reviews r ON b.Id = r.BookId
	                GROUP BY b.Id
	                ORDER BY AverageRating DESC
                ),
                BookAuthors AS (
	                SELECT 
		                ab.BooksId,
		                STRING_AGG(a.Name, ',') AS AuthorNames
	                FROM AuthorBook ab
	                JOIN Authors a ON ab.AuthorsId = a.Id
	                JOIN BookRatings br ON ab.BooksId = br.Id
	                GROUP BY ab.BooksId
                ),
                BookGenres AS (
	                SELECT 
		                bg.BooksId,
		                STRING_AGG(g.Name, ',') AS GenreNames
	                FROM BookGenre bg
	                JOIN Genres g ON bg.GenresId = g.Id
	                JOIN BookRatings br ON bg.BooksId = br.Id
	                GROUP BY bg.BooksId
                )
                SELECT 
	                b.Id,
	                b.Title,
	                b.Price,
	                COALESCE(ba.AuthorNames, '') AS AuthorNames,
	                COALESCE(bg.GenreNames, '') AS GenreNames,
	                br.AverageRating
                FROM BookRatings br
                JOIN Books b ON b.Id = br.Id
                LEFT JOIN BookAuthors ba ON b.Id = ba.BooksId
                LEFT JOIN BookGenres bg ON b.Id = bg.BooksId
                ORDER BY br.AverageRating DESC";
                #endregion

                var results = await db.Database
                    .SqlQueryRaw<BookDetailedResponseQuery>(sql)
                    .ToListAsync();

                var books = results.Select(r => new BookDetailedResponse(
                    r.Id,
                    r.Title,
                    r.Price,
                    string.IsNullOrEmpty(r.AuthorNames)
                        ? new List<string>()
                        : r.AuthorNames.Split(',').ToList(),
                    string.IsNullOrEmpty(r.GenreNames)
                        ? new List<string>()
                        : r.GenreNames.Split(',').ToList(),
                    Math.Round(r.AverageRating, 2)
                )).ToList();

                logger.LogInformation("Retrieved top {BookCount} books by rating", books.Count);
                return books;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving top 10 books by rating");
                throw;
            }
        }

        public async Task<BookResponse?> GetByIdAsync(int id)
        {
            logger.LogInformation("Retrieving book with Id: {BookId}", id);

            try
            {
                var book = await db.Books
                    .Where(b => b.Id == id)
                    .Select(b => new BookResponse(
                        b.Id,
                        b.Title,
                        b.Price
                    ))
                    .FirstOrDefaultAsync();

                if (book is null)
                {
                    logger.LogWarning("Book with Id: {BookId} not found", id);
                    return null;
                }

                logger.LogInformation("Successfully retrieved book with Id: {BookId}", id);
                return book;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving book with Id: {BookId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<BookDetailedResponse>> SearchBooksAsync(BookSearchRequest request)
        {
            logger.LogInformation("Searching books with filters: {@SearchRequest}", request);

            try
            {
                var results = await db.Database
                    .SqlQueryRaw<BookDetailedResponseQuery>(
                        @"EXEC SearchBooks 
                            @BookTitle = {0}, 
                            @AuthorName = {1}, 
                            @GenreName = {2}, 
                            @MinPrice = {3}, 
                            @MaxPrice = {4}, 
                            @MinAverageRating = {5}",
                        request.BookTitle!,
                        request.AuthorName!,
                        request.GenreName!,
                        request.MinPrice!,
                        request.MaxPrice!,
                        request.MinAverageRating!)
                    .ToListAsync();

                var books = results.Select(r => new BookDetailedResponse(
                    r.Id,
                    r.Title,
                    r.Price,
                    string.IsNullOrEmpty(r.AuthorNames)
                        ? new List<string>()
                        : r.AuthorNames.Split(',').ToList(),
                    string.IsNullOrEmpty(r.GenreNames)
                        ? new List<string>()
                        : r.GenreNames.Split(',').ToList(),
                    Math.Round(r.AverageRating, 2)
                )).ToList();

                logger.LogInformation("Found {BookCount} books matching search criteria", books.Count);
                return books;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error searching books");
                throw;
            }
        }

        public async Task<BookDetailedResponse> CreateAsync(BookCreateRequest bookCreate)
        {
            logger.LogInformation("Creating new book: {BookTitle}", bookCreate.Title);

            try
            {
                var book = new Book
                {
                    Title = bookCreate.Title,
                    Price = bookCreate.Price
                };

                if (bookCreate.AuthorIds is not null && bookCreate.AuthorIds.Count > 0)
                {
                    var authors = await db.Authors
                        .Where(a => bookCreate.AuthorIds.Contains(a.Id))
                        .ToListAsync();

                    foreach (var author in authors)
                    {
                        book.Authors.Add(author);
                    }
                }

                if (bookCreate.GenreIds is not null && bookCreate.GenreIds.Count > 0)
                {
                    var genres = await db.Genres
                        .Where(g => bookCreate.GenreIds.Contains(g.Id))
                        .ToListAsync();

                    foreach (var genre in genres)
                    {
                        book.Genres.Add(genre);
                    }
                }

                db.Books.Add(book);
                await db.SaveChangesAsync();

                var createdBook = await db.Books
                    .Where(b => b.Id == book.Id)
                    .Select(b => new BookDetailedResponse(
                        b.Id,
                        b.Title,
                        b.Price,
                        b.Authors.Select(a => a.Name).ToList(),
                        b.Genres.Select(g => g.Name).ToList(),
                        b.Reviews.Count > 0 ? Math.Round(b.Reviews.Average(r => r.Rating), 2) : 0
                    ))
                    .FirstAsync();

                logger.LogInformation("Successfully created book with Id: {BookId}", book.Id);
                return createdBook;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating book: {BookTitle}", bookCreate.Title);
                throw;
            }
        }

        public async Task<BookResponse?> UpdateAsync(int id, BookPriceUpdateRequest priceUpdate)
        {
            logger.LogInformation("Updating price for book Id: {BookId}", id);

            try
            {
                var book = await db.Books
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (book is null)
                {
                    logger.LogWarning("Cannot update - Book with Id: {BookId} not found", id);
                    return null;
                }

                var oldPrice = book.Price;
                book.Price = priceUpdate.Price;
                await db.SaveChangesAsync();

                logger.LogInformation("Updated book price. BookId: {BookId}, OldPrice: {OldPrice}, NewPrice: {NewPrice}",
                    id, oldPrice, priceUpdate.Price);

                var updatedBook = await db.Books
                    .Where(b => b.Id == id)
                    .Select(b => new BookResponse(
                        b.Id,
                        b.Title,
                        b.Price
                    ))
                    .FirstAsync();

                return updatedBook;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating book with Id: {BookId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            logger.LogInformation("Attempting to delete book with Id: {BookId}", id);

            try
            {
                var book = await db.Books.FindAsync(id);
                if (book is null)
                {
                    logger.LogWarning("Cannot delete - Book with Id: {BookId} not found", id);
                    return false;
                }

                db.Books.Remove(book);
                var deleted = await db.SaveChangesAsync() > 0;

                if (deleted)
                {
                    logger.LogInformation("Successfully deleted book with Id: {BookId}", id);
                }

                return deleted;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting book with Id: {BookId}", id);
                throw;
            }
        }

        public async Task<BookDetailedResponse?> UpdateAuthorsAsync(int id, BookAuthorsUpdateRequest authorsUpdate)
        {
            logger.LogInformation("Updating authors for book Id: {BookId}", id);

            try
            {
                var book = await db.Books
                    .Include(b => b.Authors)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (book is null)
                {
                    logger.LogWarning("Cannot update authors - Book with Id: {BookId} not found", id);
                    return null;
                }

                book.Authors.Clear();

                if (authorsUpdate.AuthorIds.Count > 0)
                {
                    var authors = await db.Authors
                        .Where(a => authorsUpdate.AuthorIds.Contains(a.Id))
                        .ToListAsync();

                    foreach (var author in authors)
                    {
                        book.Authors.Add(author);
                    }
                }

                await db.SaveChangesAsync();

                logger.LogInformation("Successfully updated authors for book Id: {BookId}", id);

                var updatedBook = await db.Books
                    .Where(b => b.Id == id)
                    .Select(b => new BookDetailedResponse(
                        b.Id,
                        b.Title,
                        b.Price,
                        b.Authors.Select(a => a.Name).ToList(),
                        b.Genres.Select(g => g.Name).ToList(),
                        b.Reviews.Count > 0 ? Math.Round(b.Reviews.Average(r => r.Rating), 2) : 0
                    ))
                    .FirstAsync();

                return updatedBook;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating authors for book Id: {BookId}", id);
                throw;
            }
        }

        public async Task<BookDetailedResponse?> UpdateGenresAsync(int id, BookGenresUpdateRequest genresUpdate)
        {
            logger.LogInformation("Updating genres for book Id: {BookId}", id);

            try
            {
                var book = await db.Books
                    .Include(b => b.Genres)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (book is null)
                {
                    logger.LogWarning("Cannot update genres - Book with Id: {BookId} not found", id);
                    return null;
                }

                book.Genres.Clear();

                if (genresUpdate.GenreIds.Count > 0)
                {
                    var genres = await db.Genres
                        .Where(g => genresUpdate.GenreIds.Contains(g.Id))
                        .ToListAsync();

                    foreach (var genre in genres)
                    {
                        book.Genres.Add(genre);
                    }
                }

                await db.SaveChangesAsync();

                logger.LogInformation("Successfully updated genres for book Id: {BookId}", id);

                var updatedBook = await db.Books
                    .Where(b => b.Id == id)
                    .Select(b => new BookDetailedResponse(
                        b.Id,
                        b.Title,
                        b.Price,
                        b.Authors.Select(a => a.Name).ToList(),
                        b.Genres.Select(g => g.Name).ToList(),
                        b.Reviews.Count > 0 ? Math.Round(b.Reviews.Average(r => r.Rating), 2) : 0
                    ))
                    .FirstAsync();

                return updatedBook;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating genres for book Id: {BookId}", id);
                throw;
            }
        }
    }
}
