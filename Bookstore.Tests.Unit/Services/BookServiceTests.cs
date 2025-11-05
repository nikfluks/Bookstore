using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Application.Services;
using Bookstore.Domain.Entities;
using Bookstore.Tests.Unit.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bookstore.Tests.Unit.Services
{
    public class BookServiceTests : IDisposable
    {
        private readonly IAppDbContext _dbContext;
        private readonly IBookService _bookService;

        public BookServiceTests()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new TestDbContext(options);
            var logger = NullLogger<BookService>.Instance;
            _bookService = new BookService(_dbContext, logger);
        }

        public void Dispose()
        {
            if (_dbContext is TestDbContext context)
            {
                context.Database.EnsureDeleted();
                context.Dispose();
            }
        }

        #region GetAllDetailedAsync Tests

        [Fact]
        public async Task GetAllDetailedAsync_ShouldReturnEmptyList_WhenNoBooksExist()
        {
            var result = await _bookService.GetAllDetailedAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllDetailedAsync_ShouldReturnAllBooks_WithAuthorsAndGenres()
        {
            var author1 = new Author { Id = 1, Name = "Author 1", BirthYear = 1980 };
            var author2 = new Author { Id = 2, Name = "Author 2", BirthYear = 1990 };
            var genre1 = new Genre { Id = 1, Name = "Fiction" };
            var genre2 = new Genre { Id = 2, Name = "Mystery" };

            var book1 = new Book { Id = 1, Title = "Book 1", Price = 10.99f };
            book1.Authors.Add(author1);
            book1.Genres.Add(genre1);

            var book2 = new Book { Id = 2, Title = "Book 2", Price = 15.99f };
            book2.Authors.Add(author2);
            book2.Genres.Add(genre2);

            _dbContext.Authors.AddRange(author1, author2);
            _dbContext.Genres.AddRange(genre1, genre2);
            _dbContext.Books.AddRange(book1, book2);
            await _dbContext.SaveChangesAsync();

            var result = await _bookService.GetAllDetailedAsync();

            var booksList = result.ToList();
            booksList.Should().HaveCount(2);
            booksList[0].Title.Should().Be("Book 1");
            booksList[0].AuthorNames.Should().ContainSingle().Which.Should().Be("Author 1");
            booksList[0].GenreNames.Should().ContainSingle().Which.Should().Be("Fiction");
            booksList[0].AverageRating.Should().Be(0);
        }

        [Fact]
        public async Task GetAllDetailedAsync_ShouldCalculateAverageRating_WhenReviewsExist()
        {
            var author = new Author { Id = 1, Name = "Author 1", BirthYear = 1980 };
            var book = new Book { Id = 1, Title = "Book 1", Price = 10.99f };
            book.Authors.Add(author);

            var review1 = new Review { Id = 1, Rating = 4, Book = book };
            var review2 = new Review { Id = 2, Rating = 5, Book = book };

            _dbContext.Authors.Add(author);
            _dbContext.Books.Add(book);
            _dbContext.Reviews.AddRange(review1, review2);
            await _dbContext.SaveChangesAsync();

            var result = await _bookService.GetAllDetailedAsync();

            var booksList = result.ToList();
            booksList.Should().ContainSingle();
            booksList[0].AverageRating.Should().Be(4.5);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenBookDoesNotExist()
        {
            var result = await _bookService.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnBook_WhenBookExists()
        {
            var author = new Author { Id = 1, Name = "Test Author", BirthYear = 1980 };
            var genre = new Genre { Id = 1, Name = "Fiction" };
            var book = new Book { Id = 1, Title = "Test Book", Price = 10.99f };
            book.Authors.Add(author);
            book.Genres.Add(genre);

            _dbContext.Authors.Add(author);
            _dbContext.Genres.Add(genre);
            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync();

            var result = await _bookService.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Title.Should().Be("Test Book");
            result.AuthorNames.Should().ContainSingle().Which.Should().Be("Test Author");
            result.GenreNames.Should().ContainSingle().Which.Should().Be("Fiction");
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ShouldCreateBook_WithoutAuthorsAndGenres()
        {
            var request = new BookCreateRequest("New Book", 19.99f);

            var result = await _bookService.CreateAsync(request);

            result.Should().NotBeNull();
            result.Title.Should().Be("New Book");
            result.AuthorNames.Should().BeEmpty();
            result.GenreNames.Should().BeEmpty();
            result.AverageRating.Should().Be(0);

            var bookInDb = await _dbContext.Books.FindAsync(result.Id);
            bookInDb.Should().NotBeNull();
            bookInDb!.Price.Should().Be(19.99f);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateBook_WithAuthorsAndGenres()
        {
            var author1 = new Author { Id = 1, Name = "Author 1", BirthYear = 1980 };
            var author2 = new Author { Id = 2, Name = "Author 2", BirthYear = 1990 };
            var genre1 = new Genre { Id = 1, Name = "Fiction" };
            var genre2 = new Genre { Id = 2, Name = "Mystery" };

            _dbContext.Authors.AddRange(author1, author2);
            _dbContext.Genres.AddRange(genre1, genre2);
            await _dbContext.SaveChangesAsync();

            var request = new BookCreateRequest
            (
                "New Book",
                19.99f,
                [1, 2],
                [1, 2]
            );

            var result = await _bookService.CreateAsync(request);

            result.Should().NotBeNull();
            result.Title.Should().Be("New Book");
            result.AuthorNames.Should().HaveCount(2);
            result.AuthorNames.Should().Contain(["Author 1", "Author 2"]);
            result.GenreNames.Should().HaveCount(2);
            result.GenreNames.Should().Contain(["Fiction", "Mystery"]);
        }

        [Fact]
        public async Task CreateAsync_ShouldIgnoreNonExistentAuthorsAndGenres()
        {
            var author = new Author { Id = 1, Name = "Author 1", BirthYear = 1980 };
            _dbContext.Authors.Add(author);
            await _dbContext.SaveChangesAsync();

            var request = new BookCreateRequest
            (
                "New Book",
                19.99f,
                new List<int> { 1, 999 }, // 999 doesn't exist
                new List<int> { 888 } // 888 doesn't exist
            );

            var result = await _bookService.CreateAsync(request);

            result.Should().NotBeNull();
            result.AuthorNames.Should().ContainSingle().Which.Should().Be("Author 1");
            result.GenreNames.Should().BeEmpty();
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenBookDoesNotExist()
        {
            var request = new BookPriceUpdateRequest(25.99f);

            var result = await _bookService.UpdateAsync(999, request);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdatePrice_WhenBookExists()
        {
            var book = new Book { Id = 1, Title = "Test Book", Price = 10.99f };
            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync();

            var request = new BookPriceUpdateRequest(25.99f);

            var result = await _bookService.UpdateAsync(1, request);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);

            var bookInDb = await _dbContext.Books.FindAsync(1);
            bookInDb!.Price.Should().Be(25.99f);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenBookDoesNotExist()
        {
            var result = await _bookService.DeleteAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenBookExists()
        {
            var book = new Book { Id = 1, Title = "Test Book", Price = 10.99f };
            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync();

            var result = await _bookService.DeleteAsync(1);

            result.Should().BeTrue();

            var bookInDb = await _dbContext.Books.FindAsync(1);
            bookInDb.Should().BeNull();
        }

        #endregion

        #region UpdateAuthorsAsync Tests

        [Fact]
        public async Task UpdateAuthorsAsync_ShouldReturnNull_WhenBookDoesNotExist()
        {
            var request = new BookAuthorsUpdateRequest(new List<int> { 1 });

            var result = await _bookService.UpdateAuthorsAsync(999, request);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAuthorsAsync_ShouldReplaceAuthors_WhenBookExists()
        {
            var author1 = new Author { Id = 1, Name = "Author 1", BirthYear = 1980 };
            var author2 = new Author { Id = 2, Name = "Author 2", BirthYear = 1990 };
            var author3 = new Author { Id = 3, Name = "Author 3", BirthYear = 2000 };

            var book = new Book { Id = 1, Title = "Test Book", Price = 10.99f };
            book.Authors.Add(author1);
            book.Authors.Add(author2);

            _dbContext.Authors.AddRange(author1, author2, author3);
            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync();

            var request = new BookAuthorsUpdateRequest([2, 3]);

            var result = await _bookService.UpdateAuthorsAsync(1, request);

            result.Should().NotBeNull();
            result!.AuthorNames.Should().HaveCount(2);
            result.AuthorNames.Should().Contain(["Author 2", "Author 3"]);
            result.AuthorNames.Should().NotContain("Author 1");
        }

        [Fact]
        public async Task UpdateAuthorsAsync_ShouldClearAuthors_WhenEmptyListProvided()
        {
            var author = new Author { Id = 1, Name = "Author 1", BirthYear = 1980 };
            var book = new Book { Id = 1, Title = "Test Book", Price = 10.99f };
            book.Authors.Add(author);

            _dbContext.Authors.Add(author);
            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync();

            var request = new BookAuthorsUpdateRequest([]);

            var result = await _bookService.UpdateAuthorsAsync(1, request);

            result.Should().NotBeNull();
            result!.AuthorNames.Should().BeEmpty();
        }

        #endregion

        #region UpdateGenresAsync Tests

        [Fact]
        public async Task UpdateGenresAsync_ShouldReturnNull_WhenBookDoesNotExist()
        {
            var request = new BookGenresUpdateRequest([1]);

            var result = await _bookService.UpdateGenresAsync(999, request);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateGenresAsync_ShouldReplaceGenres_WhenBookExists()
        {
            var genre1 = new Genre { Id = 1, Name = "Fiction" };
            var genre2 = new Genre { Id = 2, Name = "Mystery" };
            var genre3 = new Genre { Id = 3, Name = "Thriller" };

            var book = new Book { Id = 1, Title = "Test Book", Price = 10.99f };
            book.Genres.Add(genre1);
            book.Genres.Add(genre2);

            _dbContext.Genres.AddRange(genre1, genre2, genre3);
            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync();

            var request = new BookGenresUpdateRequest([2, 3]);

            var result = await _bookService.UpdateGenresAsync(1, request);

            result.Should().NotBeNull();
            result!.GenreNames.Should().HaveCount(2);
            result.GenreNames.Should().Contain(["Mystery", "Thriller"]);
            result.GenreNames.Should().NotContain("Fiction");
        }

        [Fact]
        public async Task UpdateGenresAsync_ShouldClearGenres_WhenEmptyListProvided()
        {
            var genre = new Genre { Id = 1, Name = "Fiction" };
            var book = new Book { Id = 1, Title = "Test Book", Price = 10.99f };
            book.Genres.Add(genre);

            _dbContext.Genres.Add(genre);
            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync();

            var request = new BookGenresUpdateRequest([]);

            var result = await _bookService.UpdateGenresAsync(1, request);

            result.Should().NotBeNull();
            result!.GenreNames.Should().BeEmpty();
        }

        #endregion
    }
}
