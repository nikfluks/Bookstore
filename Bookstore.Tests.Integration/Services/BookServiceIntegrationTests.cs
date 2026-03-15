using Bookstore.Application.Models;
using Bookstore.Tests.Integration.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Bookstore.Tests.Integration.Services;

[Trait("Category", "Integration")]
public class BookServiceIntegrationTests : IntegrationTestBase
{
    #region Helper Methods

    private async Task<AuthorResponse> CreateAuthorAsync(string name, int birthYear)
    {
        return await PostAsync<AuthorResponse>("authors", new AuthorCreateRequest(name, birthYear));
    }

    private async Task<GenreResponse> CreateGenreAsync(string name)
    {
        return await PostAsync<GenreResponse>("genres", new GenreCreateRequest(name));
    }

    private async Task<BookDetailedResponse> CreateBookAsync(string title, float price, int[] authorIds, int[] genreIds)
    {
        var request = new BookCreateRequest(title, price, authorIds, genreIds);
        return await PostAsync<BookDetailedResponse>("books", request);
    }

    private async Task<ReviewResponse> CreateReviewAsync(int bookId, int rating, string? description = null)
    {
        var request = new ReviewCreateRequest(description, rating, bookId);
        return await PostAsync<ReviewResponse>("reviews", request);
    }

    #endregion

    #region GetTop10ByRatingAsync Tests

    [Fact]
    public async Task GetTop10ByRatingAsync_ShouldReturnEmptyList_WhenNoBooksExist()
    {
        var result = await GetListAsync<BookDetailedResponse>("books/top-10");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTop10ByRatingAsync_ShouldReturnBooksOrderedByAverageRating()
    {
        var author = await CreateAuthorAsync("Test Author", 1980);
        var genre = await CreateGenreAsync("Fiction");

        var book1 = await CreateBookAsync("Low Rated Book", 10.99f, [author.Id], [genre.Id]);
        var book2 = await CreateBookAsync("High Rated Book", 15.99f, [author.Id], [genre.Id]);
        var book3 = await CreateBookAsync("Medium Rated Book", 12.99f, [author.Id], [genre.Id]);

        await CreateReviewAsync(book1.Id, 2);
        await CreateReviewAsync(book2.Id, 5);
        await CreateReviewAsync(book2.Id, 5);
        await CreateReviewAsync(book3.Id, 3);
        await CreateReviewAsync(book3.Id, 4);

        var result = await GetListAsync<BookDetailedResponse>("books/top-10");

        result.Should().HaveCount(3);
        result[0].Title.Should().Be("High Rated Book");
        result[0].AverageRating.Should().Be(5.0);
        result[1].Title.Should().Be("Medium Rated Book");
        result[1].AverageRating.Should().Be(3.5);
        result[2].Title.Should().Be("Low Rated Book");
        result[2].AverageRating.Should().Be(2.0);
    }

    [Fact]
    public async Task GetTop10ByRatingAsync_ShouldLimitResultsTo10Books()
    {
        var author = await CreateAuthorAsync("Test Author", 1980);

        var bookIds = new List<int>();
        for (var i = 1; i <= 15; i++)
        {
            var book = await CreateBookAsync($"Book {i}", 10.99f, [author.Id], []);
            bookIds.Add(book.Id);
        }

        for (var i = 0; i < 15; i++)
        {
            await CreateReviewAsync(bookIds[i], 5 - (i / 3));
        }

        var result = await GetListAsync<BookDetailedResponse>("books/top-10");

        result.Should().HaveCount(10);
    }

    #endregion

    #region CRUD & Status Code Tests

    [Fact]
    public async Task CreateBook_ShouldReturn201Created()
    {
        var author = await CreateAuthorAsync("Author", 1980);
        var genre = await CreateGenreAsync("Genre");

        using var response = await SendAsync(HttpMethod.Post, "books",
            new BookCreateRequest("New Book", 19.99f, [author.Id], [genre.Id]));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var created = await response.Content.ReadFromJsonAsync<BookDetailedResponse>();
        created!.Title.Should().Be("New Book");
    }

    [Fact]
    public async Task GetBookById_ShouldReturn200WithBook()
    {
        var author = await CreateAuthorAsync("Author", 1985);
        var book = await CreateBookAsync("Existing Book", 14.99f, [author.Id], []);

        using var response = await SendAsync(HttpMethod.Get, $"books/{book.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BookResponse>();
        result!.Title.Should().Be("Existing Book");
    }

    [Fact]
    public async Task GetBookById_ShouldReturn404_WhenBookDoesNotExist()
    {
        using var response = await SendAsync(HttpMethod.Get, "books/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateBookPrice_ShouldReturn200WithUpdatedPrice()
    {
        var author = await CreateAuthorAsync("Author", 1985);
        var book = await CreateBookAsync("Priced Book", 10.00f, [author.Id], []);

        using var response = await SendAsync(HttpMethod.Put, $"books/{book.Id}",
            new BookPriceUpdateRequest(25.00f));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BookResponse>();
        result!.Price.Should().Be(25.00f);
    }

    [Fact]
    public async Task UpdateBookPrice_ShouldReturn404_WhenBookDoesNotExist()
    {
        using var response = await SendAsync(HttpMethod.Put, "books/999999",
            new BookPriceUpdateRequest(25.00f));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_ShouldReturn204NoContent()
    {
        var author = await CreateAuthorAsync("Author", 1985);
        var book = await CreateBookAsync("To Delete", 10.00f, [author.Id], []);

        using var response = await SendAsync(HttpMethod.Delete, $"books/{book.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteBook_ShouldReturn404_WhenBookDoesNotExist()
    {
        using var response = await SendAsync(HttpMethod.Delete, "books/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllDetailed_ShouldReturnPagedResponse()
    {
        var author = await CreateAuthorAsync("Author", 1980);
        var genre = await CreateGenreAsync("Genre");
        var book = await CreateBookAsync("Detailed Book", 10.99f, [author.Id], [genre.Id]);

        await CreateReviewAsync(book.Id, 4);

        var result = await GetAsync<PagedResponse<BookDetailedResponse>>("books/details");

        result.Items.Should().ContainSingle();
        result.Items.First().Title.Should().Be("Detailed Book");
        result.Items.First().AverageRating.Should().Be(4.0);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task AnonymousRequest_ShouldReturn401Unauthorized()
    {
        ClearAuthentication();

        using var response = await SendAsync(HttpMethod.Get, "books/top-10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AnonymousPost_ShouldReturn401Unauthorized()
    {
        ClearAuthentication();

        using var response = await SendAsync(HttpMethod.Post, "books",
            new BookCreateRequest("Anon Book", 10.00f));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Reader Role Authorization Tests

    [Fact]
    public async Task ReaderCanGetBooks()
    {
        await AuthenticateAsReaderAsync();

        using var response = await SendAsync(HttpMethod.Get, "books/top-10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReaderCannotCreateBook_ShouldReturn403Forbidden()
    {
        var author = await CreateAuthorAsync("Author", 1980);

        await AuthenticateAsReaderAsync();

        using var response = await SendAsync(HttpMethod.Post, "books",
            new BookCreateRequest("Forbidden Book", 10.00f, [author.Id]));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReaderCannotUpdateBook_ShouldReturn403Forbidden()
    {
        var author = await CreateAuthorAsync("Author", 1985);
        var book = await CreateBookAsync("Book", 10.00f, [author.Id], []);

        await AuthenticateAsReaderAsync();

        using var response = await SendAsync(HttpMethod.Put, $"books/{book.Id}",
            new BookPriceUpdateRequest(99.00f));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReaderCannotDeleteBook_ShouldReturn403Forbidden()
    {
        var author = await CreateAuthorAsync("Author", 1985);
        var book = await CreateBookAsync("Book", 10.00f, [author.Id], []);

        await AuthenticateAsReaderAsync();

        using var response = await SendAsync(HttpMethod.Delete, $"books/{book.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReaderCannotCreateReview_ShouldReturn403Forbidden()
    {
        var author = await CreateAuthorAsync("Author", 1985);
        var book = await CreateBookAsync("Book", 10.00f, [author.Id], []);

        await AuthenticateAsReaderAsync();

        using var response = await SendAsync(HttpMethod.Post, "reviews",
            new ReviewCreateRequest(null, 5, book.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Database Isolation Test

    [Fact]
    public async Task DatabaseCleanup_ShouldIsolateTests()
    {
        var booksResponse = await GetListAsync<BookDetailedResponse>("books/top-10");
        var authorsResponse = await GetListAsync<AuthorResponse>("authors");
        var genresResponse = await GetListAsync<GenreResponse>("genres");
        var reviewsResponse = await GetListAsync<ReviewResponse>("reviews");

        booksResponse.Should().BeEmpty("Each test should start with a clean database");
        authorsResponse.Should().BeEmpty("Each test should start with a clean database");
        genresResponse.Should().BeEmpty("Each test should start with a clean database");
        reviewsResponse.Should().BeEmpty("Each test should start with a clean database");
    }

    #endregion
}
