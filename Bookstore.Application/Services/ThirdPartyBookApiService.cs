using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
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
            logger.LogInformation("Simulating fetch from third-party API...");

            const int totalBooks = 100000;
            var books = new List<ImportBookDto>(totalBooks);
            var random = new Random(42); // Fixed seed for reproducibility

            for (int i = 0; i < totalBooks; i++)
            {
                var title = $"{i + 1} {Titles[random.Next(Titles.Length)]}";
                var price = (float)Math.Round(random.NextDouble() * 100 * 1.25, 2);

                var authorCount = random.Next(1, 4);
                var authorNames = Enumerable.Range(0, authorCount)
                    .Select(_ => Authors[random.Next(Authors.Length)])
                    .Distinct()
                    .ToList();

                var genreCount = random.Next(1, 4);
                var genreNames = Enumerable.Range(0, genreCount)
                    .Select(_ => Genres[random.Next(Genres.Length)])
                    .Distinct()
                    .ToList();

                books.Add(new ImportBookDto(title, price, authorNames, genreNames));
            }

            logger.LogInformation("Simulated API returned {BookCount} books", books.Count);
            return await Task.FromResult<IEnumerable<ImportBookDto>>(books);
        }
    }
}
