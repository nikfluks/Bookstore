using System.Security.Cryptography;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services;

internal class ThirdPartyBookApiService(ILogger<ThirdPartyBookApiService> logger) : IThirdPartyBookApiService
{
    private static readonly string[] Titles =
    [
        "The Great Adventure", "Mystery of the Night", "Journey to the Stars",
        "Code Complete", "Clean Code", "The Pragmatic Programmer",
        "Design Patterns", "Refactoring", "Domain-Driven Design",
        "The Phoenix Project", "The DevOps Handbook", "Accelerate",
        "The Lean Startup", "Zero to One", "The Innovator's Dilemma",
        "Thinking, Fast and Slow", "Atomic Habits", "Deep Work",
        "The Art of War", "Sapiens", "Homo Deus"
    ];

    private static readonly string[] Authors =
    [
        "John Smith", "Jane Doe", "Robert Martin", "Martin Fowler",
        "Eric Evans", "Kent Beck", "Steve McConnell", "Andrew Hunt",
        "David Thomas", "Gene Kim", "Jez Humble", "Nicole Forsgren",
        "Eric Ries", "Peter Thiel", "Clayton Christensen",
        "Daniel Kahneman", "James Clear", "Cal Newport",
        "Sun Tzu", "Yuval Noah Harari"
    ];

    private static readonly string[] Genres =
    [
        "Fiction", "Mystery", "Science Fiction", "Fantasy",
        "Technology", "Programming", "Business", "Self-Help",
        "History", "Philosophy", "Biography", "Non-Fiction"
    ];

    public async Task<IEnumerable<ImportBookDto>> FetchBooksAsync()
    {
        logger.LogInformation("Simulating fetch from third-party API start...");

        const int TotalBooks = 100000;
        var books = new List<ImportBookDto>(TotalBooks);

        for (var i = 0; i < TotalBooks; i++)
        {
            var title = Titles[RandomNumberGenerator.GetInt32(Titles.Length)];
            var price = (float)Math.Round(RandomNumberGenerator.GetInt32(0, 12501) / 100.0, 2, MidpointRounding.AwayFromZero);

            var authorCount = RandomNumberGenerator.GetInt32(1, 4);
            var authorNames = Enumerable.Range(0, authorCount)
                .Select(_ => Authors[RandomNumberGenerator.GetInt32(Authors.Length)])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var genreCount = RandomNumberGenerator.GetInt32(1, 4);
            var genreNames = Enumerable.Range(0, genreCount)
                .Select(_ => Genres[RandomNumberGenerator.GetInt32(Genres.Length)])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            books.Add(new ImportBookDto(title, price, authorNames, genreNames));
        }

        logger.LogInformation("Simulated fetch from third-party API completed. Returned {BookCount} books", books.Count);
        return await Task.FromResult<IEnumerable<ImportBookDto>>(books);
    }
}
