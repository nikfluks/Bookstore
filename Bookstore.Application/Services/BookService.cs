using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Services
{
    internal class BookService : IBookService
    {
        private readonly AppDbContext db;

        public BookService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<IEnumerable<BookDetailedResponse>> GetAllDetailedAsync()
        {
            return await db.Books
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .Include(b => b.Reviews)
                .Select(b => new BookDetailedResponse(
                    b.Id,
                    b.Title,
                    b.Authors.Select(a => a.Name).ToList(),
                    b.Genres.Select(g => g.Name).ToList(),
                    b.Reviews.Any() ? Math.Round(b.Reviews.Average(r => r.Rating), 2) : 0
                ))
                .ToListAsync();
        }

        public async Task<IEnumerable<BookDetailedResponse>> GetTop10ByRatingAsync()
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

            return results.Select(r => new BookDetailedResponse(
                r.Id,
                r.Title,
                string.IsNullOrEmpty(r.AuthorNames)
                    ? new List<string>()
                    : r.AuthorNames.Split(',').ToList(),
                string.IsNullOrEmpty(r.GenreNames)
                    ? new List<string>()
                    : r.GenreNames.Split(',').ToList(),
                Math.Round(r.AverageRating, 2)
            )).ToList();
        }

        public async Task<BookDetailedResponse?> GetByIdAsync(int id)
        {
            var book = await db.Books
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == id);

            return book is null
                ? null
                : new BookDetailedResponse(
                    book.Id,
                    book.Title,
                    book.Authors.Select(a => a.Name).ToList(),
                    book.Genres.Select(g => g.Name).ToList(),
                    book.Reviews.Any() ? Math.Round(book.Reviews.Average(r => r.Rating), 2) : 0
                );
        }

        public async Task<BookDetailedResponse> CreateAsync(BookCreateRequest bookCreate)
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
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .Include(b => b.Reviews)
                .FirstAsync(b => b.Id == book.Id);

            return new BookDetailedResponse(
                createdBook.Id,
                createdBook.Title,
                createdBook.Authors.Select(a => a.Name).ToList(),
                createdBook.Genres.Select(g => g.Name).ToList(),
                createdBook.Reviews.Any() ? Math.Round(createdBook.Reviews.Average(r => r.Rating), 2) : 0
            );
        }

        public async Task<BookDetailedResponse?> UpdateAsync(int id, BookPriceUpdateRequest priceUpdate)
        {
            var book = await db.Books
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book is null) return null;

            book.Price = priceUpdate.Price;
            await db.SaveChangesAsync();

            return new BookDetailedResponse(
                book.Id,
                book.Title,
                book.Authors.Select(a => a.Name).ToList(),
                book.Genres.Select(g => g.Name).ToList(),
                book.Reviews.Any() ? Math.Round(book.Reviews.Average(r => r.Rating), 2) : 0
            );
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await db.Books.FindAsync(id);
            if (book is null) return false;

            db.Books.Remove(book);
            return await db.SaveChangesAsync() > 0;
        }

        public async Task<BookDetailedResponse?> UpdateAuthorsAsync(int id, BookAuthorsUpdateRequest authorsUpdate)
        {
            var book = await db.Books
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book is null) return null;

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

            return new BookDetailedResponse(
                book.Id,
                book.Title,
                book.Authors.Select(a => a.Name).ToList(),
                book.Genres.Select(g => g.Name).ToList(),
                book.Reviews.Any() ? Math.Round(book.Reviews.Average(r => r.Rating), 2) : 0
            );
        }

        public async Task<BookDetailedResponse?> UpdateGenresAsync(int id, BookGenresUpdateRequest genresUpdate)
        {
            var book = await db.Books
                .Include(b => b.Authors)
                .Include(b => b.Genres)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book is null) return null;

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

            return new BookDetailedResponse(
                book.Id,
                book.Title,
                book.Authors.Select(a => a.Name).ToList(),
                book.Genres.Select(g => g.Name).ToList(),
                book.Reviews.Any() ? Math.Round(book.Reviews.Average(r => r.Rating), 2) : 0
            );
        }
    }
}
