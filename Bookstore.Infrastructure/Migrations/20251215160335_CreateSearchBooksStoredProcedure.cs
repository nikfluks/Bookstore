using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookstore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateSearchBooksStoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE SearchBooks
                    @SearchTerm NVARCHAR(100) = NULL,
                    @AuthorName NVARCHAR(100) = NULL,
                    @GenreName NVARCHAR(100) = NULL,
                    @MinPrice FLOAT = NULL,
                    @MaxPrice FLOAT = NULL,
                    @MinRating FLOAT = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;

                    WITH FilteredBooks AS (
                        SELECT DISTINCT
                            b.Id,
                            b.Title,
                            COALESCE(AVG(CAST(r.Rating AS FLOAT)), 0) AS AverageRating
                        FROM Books b
                        LEFT JOIN Reviews r ON b.Id = r.BookId
                        LEFT JOIN AuthorBook ab ON b.Id = ab.BooksId
                        LEFT JOIN Authors a ON ab.AuthorsId = a.Id
                        LEFT JOIN BookGenre bg ON b.Id = bg.BooksId
                        LEFT JOIN Genres g ON bg.GenresId = g.Id
                        WHERE 
                            (@SearchTerm IS NULL OR b.Title LIKE '%' + @SearchTerm + '%')
                            AND (@AuthorName IS NULL OR a.Name LIKE '%' + @AuthorName + '%')
                            AND (@GenreName IS NULL OR g.Name LIKE '%' + @GenreName + '%')
                            AND (@MinPrice IS NULL OR b.Price >= @MinPrice)
                            AND (@MaxPrice IS NULL OR b.Price <= @MaxPrice)
                        GROUP BY b.Id, b.Title, b.Price
                        HAVING (@MinRating IS NULL OR COALESCE(AVG(CAST(r.Rating AS FLOAT)), 0) >= @MinRating)
                    ),
                    BookAuthors AS (
                        SELECT 
                            fb.Id,
                            STRING_AGG(a.Name, ',') AS AuthorNames
                        FROM FilteredBooks fb
                        LEFT JOIN AuthorBook ab ON fb.Id = ab.BooksId
                        LEFT JOIN Authors a ON ab.AuthorsId = a.Id
                        GROUP BY fb.Id
                    ),
                    BookGenres AS (
                        SELECT 
                            fb.Id,
                            STRING_AGG(g.Name, ',') AS GenreNames
                        FROM FilteredBooks fb
                        LEFT JOIN BookGenre bg ON fb.Id = bg.BooksId
                        LEFT JOIN Genres g ON bg.GenresId = g.Id
                        GROUP BY fb.Id
                    )
                    SELECT 
                        fb.Id,
                        fb.Title,
                        COALESCE(ba.AuthorNames, '') AS AuthorNames,
                        COALESCE(bg.GenreNames, '') AS GenreNames,
                        fb.AverageRating
                    FROM FilteredBooks fb
                    LEFT JOIN BookAuthors ba ON fb.Id = ba.Id
                    LEFT JOIN BookGenres bg ON fb.Id = bg.Id
                    ORDER BY fb.AverageRating DESC, fb.Title
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SearchBooks");
        }
    }
}
