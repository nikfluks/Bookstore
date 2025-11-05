using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Application.Services;
using Bookstore.Domain.Entities;
using Bookstore.Tests.Integration.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bookstore.Tests.Integration.Services
{
    [Trait("Category", "Integration")]
    public class BookServiceIntegrationTests : IntegrationTestBase
    {
        private readonly IBookService _bookService;

        public BookServiceIntegrationTests()
        {
            var logger = NullLogger<BookService>.Instance;
            _bookService = new BookService(DbContext, logger);
        }

        #region GetTop10ByRatingAsync Tests

        [Fact]
        public async Task GetTop10ByRatingAsync_ShouldReturnEmptyList_WhenNoBooksExist()
        {
            var result = await _bookService.GetTop10ByRatingAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTop10ByRatingAsync_ShouldReturnBooksOrderedByRating()
        {
            var author = new Author { Name = "Test Author", BirthYear = 1980 };
            var genre = new Genre { Name = "Fiction" };

            var book1 = new Book { Title = "Low Rated Book", Price = 10.99f };
            book1.Authors.Add(author);
            book1.Genres.Add(genre);

            var book2 = new Book { Title = "High Rated Book", Price = 15.99f };
            book2.Authors.Add(author);
            book2.Genres.Add(genre);

            var book3 = new Book { Title = "Medium Rated Book", Price = 12.99f };
            book3.Authors.Add(author);
            book3.Genres.Add(genre);

            DbContext.Authors.Add(author);
            DbContext.Genres.Add(genre);
            DbContext.Books.AddRange(book1, book2, book3);
            await DbContext.SaveChangesAsync();

            var review1 = new Review { Rating = 2, Book = book1 };
            var review2 = new Review { Rating = 5, Book = book2 };
            var review3 = new Review { Rating = 5, Book = book2 };
            var review4 = new Review { Rating = 3, Book = book3 };
            var review5 = new Review { Rating = 4, Book = book3 };

            DbContext.Reviews.AddRange(review1, review2, review3, review4, review5);
            await DbContext.SaveChangesAsync();

            var result = await _bookService.GetTop10ByRatingAsync();

            var booksList = result.ToList();
            booksList.Should().HaveCount(3);
            booksList[0].Title.Should().Be("High Rated Book");
            booksList[0].AverageRating.Should().Be(5.0);
            booksList[1].Title.Should().Be("Medium Rated Book");
            booksList[1].AverageRating.Should().Be(3.5);
            booksList[2].Title.Should().Be("Low Rated Book");
            booksList[2].AverageRating.Should().Be(2.0);
        }

        [Fact]
        public async Task GetTop10ByRatingAsync_ShouldLimitResultsTo10Books()
        {
            var author = new Author { Name = "Test Author", BirthYear = 1980 };
            DbContext.Authors.Add(author);
            await DbContext.SaveChangesAsync();

            var books = new List<Book>();
            for (int i = 1; i <= 15; i++)
            {
                var book = new Book
                {
                    Title = $"Book {i}",
                    Price = 10.99f
                };
                book.Authors.Add(author);
                books.Add(book);
            }

            DbContext.Books.AddRange(books);
            await DbContext.SaveChangesAsync();

            for (int i = 0; i < 15; i++)
            {
                var review = new Review
                {
                    // Creates ratings: 5, 5, 5, 4, 4, 4, 3, 3, 3, 2, 2, 2, 1, 1, 1
                    Rating = 5 - (i / 3),
                    Book = books[i]
                };
                DbContext.Reviews.Add(review);
            }
            await DbContext.SaveChangesAsync();

            var result = await _bookService.GetTop10ByRatingAsync();

            result.Should().HaveCount(10);
        }

        [Fact]
        public async Task GetTop10ByRatingAsync_ShouldHandleBooksWithoutReviews()
        {
            var author = new Author { Name = "Test Author", BirthYear = 1980 };
            DbContext.Authors.Add(author);
            await DbContext.SaveChangesAsync();

            var bookWithReviews = new Book { Title = "Book With Reviews", Price = 10.99f };
            bookWithReviews.Authors.Add(author);

            var bookWithoutReviews = new Book { Title = "Book Without Reviews", Price = 12.99f };
            bookWithoutReviews.Authors.Add(author);

            DbContext.Books.AddRange(bookWithReviews, bookWithoutReviews);
            await DbContext.SaveChangesAsync();

            var review = new Review { Rating = 5, Book = bookWithReviews };
            DbContext.Reviews.Add(review);
            await DbContext.SaveChangesAsync();

            var result = await _bookService.GetTop10ByRatingAsync();

            var booksList = result.ToList();
            booksList.Should().HaveCount(2);
            booksList[0].Title.Should().Be("Book With Reviews");
            booksList[0].AverageRating.Should().Be(5.0);
            booksList[1].Title.Should().Be("Book Without Reviews");
            booksList[1].AverageRating.Should().Be(0.0);
        }

        [Fact]
        public async Task GetTop10ByRatingAsync_ShouldIncludeAllAuthorsAndGenres()
        {
            var author1 = new Author { Name = "Author 1", BirthYear = 1980 };
            var author2 = new Author { Name = "Author 2", BirthYear = 1990 };
            var genre1 = new Genre { Name = "Fiction" };
            var genre2 = new Genre { Name = "Mystery" };

            var book = new Book { Title = "Multi Author Genre Book", Price = 10.99f };
            book.Authors.Add(author1);
            book.Authors.Add(author2);
            book.Genres.Add(genre1);
            book.Genres.Add(genre2);

            DbContext.Authors.AddRange(author1, author2);
            DbContext.Genres.AddRange(genre1, genre2);
            DbContext.Books.Add(book);
            await DbContext.SaveChangesAsync();

            var review = new Review { Rating = 5, Book = book };
            DbContext.Reviews.Add(review);
            await DbContext.SaveChangesAsync();

            var result = await _bookService.GetTop10ByRatingAsync();

            var booksList = result.ToList();
            booksList.Should().ContainSingle();
            booksList[0].AuthorNames.Should().HaveCount(2);
            booksList[0].AuthorNames.Should().Contain(["Author 1", "Author 2"]);
            booksList[0].GenreNames.Should().HaveCount(2);
            booksList[0].GenreNames.Should().Contain(["Fiction", "Mystery"]);
        }

        #endregion

        #region GetAllDetailedAsync Integration Tests

        [Fact]
        public async Task GetAllDetailedAsync_ShouldWorkWithRealDatabase()
        {
            var author = new Author { Name = "Integration Test Author", BirthYear = 1980 };
            var genre = new Genre { Name = "Integration Test Genre" };

            var book = new Book { Title = "Integration Test Book", Price = 10.99f };
            book.Authors.Add(author);
            book.Genres.Add(genre);

            DbContext.Authors.Add(author);
            DbContext.Genres.Add(genre);
            DbContext.Books.Add(book);
            await DbContext.SaveChangesAsync();

            var review = new Review { Rating = 4, Book = book };
            DbContext.Reviews.Add(review);
            await DbContext.SaveChangesAsync();

            var result = await _bookService.GetAllDetailedAsync();

            var booksList = result.ToList();
            booksList.Should().ContainSingle();
            booksList[0].Title.Should().Be("Integration Test Book");
            booksList[0].AverageRating.Should().Be(4.0);
        }

        #endregion

        #region Complex Scenario Tests

        [Fact]
        public async Task ComplexScenario_CreateUpdateAndVerifyRanking()
        {
            var author = new Author { Name = "Prolific Author", BirthYear = 1975 };
            var genre = new Genre { Name = "Bestseller" };

            DbContext.Authors.Add(author);
            DbContext.Genres.Add(genre);
            await DbContext.SaveChangesAsync();

            // Act - Create books through service
            var createRequest1 = new BookCreateRequest
            (
                "First Book",
                19.99f,
                [author.Id],
                [genre.Id]
            );
            var book1 = await _bookService.CreateAsync(createRequest1);

            var createRequest2 = new BookCreateRequest
            (
               "Second Book",
               24.99f,
               [author.Id],
               [genre.Id]
            );
            var book2 = await _bookService.CreateAsync(createRequest2);

            var book1Entity = await DbContext.Books.FindAsync(book1.Id);
            var book2Entity = await DbContext.Books.FindAsync(book2.Id);

            DbContext.Reviews.Add(new Review { Rating = 5, Book = book1Entity! });
            DbContext.Reviews.Add(new Review { Rating = 5, Book = book1Entity! });
            DbContext.Reviews.Add(new Review { Rating = 4, Book = book1Entity! });

            DbContext.Reviews.Add(new Review { Rating = 3, Book = book2Entity! });
            DbContext.Reviews.Add(new Review { Rating = 3, Book = book2Entity! });
            await DbContext.SaveChangesAsync();

            var topBooks = await _bookService.GetTop10ByRatingAsync();

            var topBooksList = topBooks.ToList();
            topBooksList.Should().HaveCount(2);
            topBooksList[0].Title.Should().Be("First Book");
            topBooksList[0].AverageRating.Should().BeApproximately(4.67, 0.01);
            topBooksList[1].Title.Should().Be("Second Book");
            topBooksList[1].AverageRating.Should().Be(3.0);
        }

        [Fact]
        public async Task DatabaseCleanup_ShouldIsolateTests()
        {
            var books = await DbContext.Books.ToListAsync();
            var authors = await DbContext.Authors.ToListAsync();
            var genres = await DbContext.Genres.ToListAsync();
            var reviews = await DbContext.Reviews.ToListAsync();

            books.Should().BeEmpty("Each test should start with a clean database");
            authors.Should().BeEmpty("Each test should start with a clean database");
            genres.Should().BeEmpty("Each test should start with a clean database");
            reviews.Should().BeEmpty("Each test should start with a clean database");
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetTop10ByRatingAsync_ShouldHandleIdenticalRatings()
        {
            var author = new Author { Name = "Test Author", BirthYear = 1980 };
            DbContext.Authors.Add(author);
            await DbContext.SaveChangesAsync();

            var book1 = new Book { Title = "Book A", Price = 10.99f };
            book1.Authors.Add(author);
            var book2 = new Book { Title = "Book B", Price = 10.99f };
            book2.Authors.Add(author);
            var book3 = new Book { Title = "Book C", Price = 10.99f };
            book3.Authors.Add(author);

            DbContext.Books.AddRange(book1, book2, book3);
            await DbContext.SaveChangesAsync();

            DbContext.Reviews.Add(new Review { Rating = 4, Book = book1 });
            DbContext.Reviews.Add(new Review { Rating = 4, Book = book2 });
            DbContext.Reviews.Add(new Review { Rating = 4, Book = book3 });
            await DbContext.SaveChangesAsync();

            var result = await _bookService.GetTop10ByRatingAsync();

            var booksList = result.ToList();
            booksList.Should().HaveCount(3);
            booksList.Should().OnlyContain(b => b.AverageRating == 4.0);
        }

        [Fact]
        public async Task GetTop10ByRatingAsync_ShouldHandleBookWithMultipleReviews()
        {
            var author = new Author { Name = "Test Author", BirthYear = 1980 };
            DbContext.Authors.Add(author);
            await DbContext.SaveChangesAsync();

            var book = new Book { Title = "Popular Book", Price = 10.99f };
            book.Authors.Add(author);
            DbContext.Books.Add(book);
            await DbContext.SaveChangesAsync();

            var ratings = new[] { 5, 5, 4, 4, 4, 3, 3, 2, 5, 4 };
            foreach (var rating in ratings)
            {
                DbContext.Reviews.Add(new Review { Rating = rating, Book = book });
            }
            await DbContext.SaveChangesAsync();

            var result = await _bookService.GetTop10ByRatingAsync();

            var booksList = result.ToList();
            booksList.Should().ContainSingle();
            // Average should be (5+5+4+4+4+3+3+2+5+4)/10 = 39/10 = 3.9
            booksList[0].AverageRating.Should().Be(3.9);
        }

        #endregion
    }
}
