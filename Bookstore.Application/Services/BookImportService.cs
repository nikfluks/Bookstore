using Bookstore.Application.Interfaces;
using Bookstore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
    internal class BookImportService(
        IAppDbContext db,
        IThirdPartyBookApiService thirdPartyApi,
        ILogger<BookImportService> logger) : IBookImportService
    {
        public async Task<int> ImportBooksAsync()
        {
            logger.LogInformation("Starting book import process...");
            var startTime = DateTime.UtcNow;

            try
            {
                var importBooks = await thirdPartyApi.FetchBooksAsync();
                var importBooksList = importBooks.ToList();
                logger.LogInformation("Received {Count} books from third-party API", importBooksList.Count);

                var existingTitles = await db.Books
                    .AsNoTracking()
                    .Select(b => b.Title.Trim().ToLower())
                    .ToHashSetAsync();

                logger.LogInformation("Found {Count} existing books in database", existingTitles.Count);

                var booksToImport = importBooksList
                    .Where(b => !existingTitles.Contains(b.Title.Trim().ToLower()))
                    .ToList();

                if (booksToImport.Count == 0)
                {
                    logger.LogInformation("No new books to import");
                    return booksToImport.Count;
                }

                var skippedCount = importBooksList.Count - booksToImport.Count;
                logger.LogInformation("Skipping {Count} duplicate books, importing {ImportCount} new books",
                        skippedCount, booksToImport.Count);

                var allAuthorNames = booksToImport
                    .SelectMany(b => b.AuthorNames)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var allGenreNames = booksToImport
                    .SelectMany(b => b.GenreNames)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                logger.LogInformation("Found {AuthorsCount} authors and {GenresCount} genres to process",
                    allAuthorNames.Count, allGenreNames.Count);

                var authorDict = await GetOrCreateAuthorsAsync(allAuthorNames);
                var genreDict = await GetOrCreateGenresAsync(allGenreNames);

                var booksToAdd = new List<Book>();

                logger.LogInformation("Mapping imported books to domain entities...");

                foreach (var importBook in booksToImport)
                {
                    var book = new Book
                    {
                        Title = importBook.Title.Trim(),
                        Price = importBook.Price
                    };

                    foreach (var authorName in importBook.AuthorNames)
                    {
                        if (authorDict.TryGetValue(authorName.ToLower(), out var author))
                        {
                            book.Authors.Add(author);
                        }
                    }

                    foreach (var genreName in importBook.GenreNames)
                    {
                        if (genreDict.TryGetValue(genreName.ToLower(), out var genre))
                        {
                            book.Genres.Add(genre);
                        }
                    }

                    booksToAdd.Add(book);
                }

                logger.LogInformation("Adding imported books to context...");
                db.Books.AddRange(booksToAdd);

                logger.LogInformation("Saving imported books to database...");
                await db.SaveChangesAsync();

                var duration = DateTime.UtcNow - startTime;
                logger.LogInformation(
                    "Book import completed successfully. Imported {Count} books in {Duration}",
                    booksToAdd.Count, duration.ToString(@"hh\:mm\:ss"));

                return booksToAdd.Count;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during book import process");
                throw;
            }
        }

        private async Task<Dictionary<string, Author>> GetOrCreateAuthorsAsync(List<string> authorNames)
        {
            var authorDict = new Dictionary<string, Author>(StringComparer.OrdinalIgnoreCase);

            var existingAuthors = await db.Authors
                .Where(a => authorNames.Contains(a.Name))
                .ToListAsync();

            foreach (var author in existingAuthors)
            {
                authorDict[author.Name.ToLower()] = author;
            }

            var existingAuthorNames = existingAuthors.Select(a => a.Name.ToLower()).ToHashSet();
            var newAuthorNames = authorNames
                .Where(name => !existingAuthorNames.Contains(name.ToLower()))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (newAuthorNames.Count > 0)
            {
                logger.LogInformation("Creating {Count} new authors", newAuthorNames.Count);

                var newAuthors = newAuthorNames.Select(name => new Author
                {
                    Name = name,
                    BirthYear = 1970
                }).ToList();

                db.Authors.AddRange(newAuthors);

                foreach (var author in newAuthors)
                {
                    authorDict[author.Name.ToLower()] = author;
                }

                logger.LogInformation("Created {Count} new authors", newAuthors.Count);
            }

            return authorDict;
        }

        private async Task<Dictionary<string, Genre>> GetOrCreateGenresAsync(List<string> genreNames)
        {
            var genreDict = new Dictionary<string, Genre>(StringComparer.OrdinalIgnoreCase);

            var existingGenres = await db.Genres
                .Where(g => genreNames.Contains(g.Name))
                .ToListAsync();

            foreach (var genre in existingGenres)
            {
                genreDict[genre.Name.ToLower()] = genre;
            }

            var existingGenreNames = existingGenres.Select(g => g.Name.ToLower()).ToHashSet();
            var newGenreNames = genreNames
                .Where(name => !existingGenreNames.Contains(name.ToLower()))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (newGenreNames.Count > 0)
            {
                logger.LogInformation("Creating {Count} new genres", newGenreNames.Count);

                var newGenres = newGenreNames.Select(name => new Genre
                {
                    Name = name
                }).ToList();

                db.Genres.AddRange(newGenres);

                foreach (var genre in newGenres)
                {
                    genreDict[genre.Name.ToLower()] = genre;
                }

                logger.LogInformation("Created {Count} new genres", newGenres.Count);
            }

            return genreDict;
        }
    }
}
