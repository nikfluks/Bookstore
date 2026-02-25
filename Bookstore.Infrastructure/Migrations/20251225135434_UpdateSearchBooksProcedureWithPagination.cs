using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookstore.Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdateSearchBooksProcedureWithPagination : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE SearchBooks
                    @BookTitle NVARCHAR(500) = NULL,
                    @AuthorName NVARCHAR(200) = NULL,
                    @GenreName NVARCHAR(100) = NULL,
                    @MinPrice FLOAT = NULL,
                    @MaxPrice FLOAT = NULL,
                    @MinAverageRating FLOAT = NULL,
                    @PageNumber INT = 1,
                    @PageSize INT = 20 -- @PageSize default should match Constants.Pagination.DefaultPageSize
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @FilteredBookIds TABLE (Id INT PRIMARY KEY);

                    IF @BookTitle IS NOT NULL
                    BEGIN
                        INSERT INTO @FilteredBookIds (Id)
                        SELECT b.Id
                        FROM Books b
                        WHERE FREETEXT(b.Title, @BookTitle);
                    END;

                    WITH FilteredBooks AS (
                        SELECT
                            b.Id,
                            b.Title,
                            b.Price,
                            COALESCE(AVG(CAST(r.Rating AS FLOAT)), 0) AS AverageRating
                        FROM Books b
                        LEFT JOIN Reviews r ON b.Id = r.BookId
                        LEFT JOIN AuthorBook ab ON b.Id = ab.BooksId
                        LEFT JOIN Authors a ON ab.AuthorsId = a.Id
                        LEFT JOIN BookGenre bg ON b.Id = bg.BooksId
                        LEFT JOIN Genres g ON bg.GenresId = g.Id
                        WHERE 
                            (@BookTitle IS NULL OR b.Id IN (SELECT Id FROM @FilteredBookIds))
                            AND (@AuthorName IS NULL OR a.Name LIKE '%' + @AuthorName + '%')
                            AND (@GenreName IS NULL OR g.Name LIKE '%' + @GenreName + '%')
                            AND (@MinPrice IS NULL OR b.Price >= @MinPrice)
                            AND (@MaxPrice IS NULL OR b.Price <= @MaxPrice)
                        GROUP BY b.Id, b.Title, b.Price
                        HAVING (@MinAverageRating IS NULL OR COALESCE(AVG(CAST(r.Rating AS FLOAT)), 0) >= @MinAverageRating)
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
                    ),
                    PaginatedBooks AS (
                        SELECT 
                            fb.Id,
                            fb.Title,
                            fb.Price,
                            COALESCE(ba.AuthorNames, '') AS AuthorNames,
                            COALESCE(bg.GenreNames, '') AS GenreNames,
                            fb.AverageRating,
                            ROW_NUMBER() OVER (ORDER BY fb.AverageRating DESC, fb.Title) AS RowNum,
                            COUNT(*) OVER () AS TotalCount
                        FROM FilteredBooks fb
                        LEFT JOIN BookAuthors ba ON fb.Id = ba.Id
                        LEFT JOIN BookGenres bg ON fb.Id = bg.Id
                    )
                    SELECT 
                        pb.Id,
                        pb.Title,
                        pb.Price,
                        pb.AuthorNames,
                        pb.GenreNames,
                        pb.AverageRating,
                        pb.TotalCount
                    FROM PaginatedBooks pb
                    WHERE pb.RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize)
                    ORDER BY pb.RowNum
                END
            ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE SearchBooks
                    @BookTitle NVARCHAR(500) = NULL,
                    @AuthorName NVARCHAR(200) = NULL,
                    @GenreName NVARCHAR(100) = NULL,
                    @MinPrice FLOAT = NULL,
                    @MaxPrice FLOAT = NULL,
                    @MinAverageRating FLOAT = NULL,
                    @UseFreeText BIT = 1
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @FilteredBookIds TABLE (Id INT PRIMARY KEY);

                    IF @BookTitle IS NOT NULL
                    BEGIN
                        IF @UseFreeText = 1
                        BEGIN
                            INSERT INTO @FilteredBookIds (Id)
                            SELECT b.Id
                            FROM Books b
                            WHERE FREETEXT(b.Title, @BookTitle);
                        END
                        ELSE
                        BEGIN
                            INSERT INTO @FilteredBookIds (Id)
                            SELECT b.Id
                            FROM Books b
                            WHERE CONTAINS(b.Title, @BookTitle);
                        END
                    END;

                    WITH FilteredBooks AS (
                        SELECT
                            b.Id,
                            b.Title,
                            b.Price,
                            COALESCE(AVG(CAST(r.Rating AS FLOAT)), 0) AS AverageRating
                        FROM Books b
                        LEFT JOIN Reviews r ON b.Id = r.BookId
                        LEFT JOIN AuthorBook ab ON b.Id = ab.BooksId
                        LEFT JOIN Authors a ON ab.AuthorsId = a.Id
                        LEFT JOIN BookGenre bg ON b.Id = bg.BooksId
                        LEFT JOIN Genres g ON bg.GenresId = g.Id
                        WHERE 
                            (@BookTitle IS NULL OR b.Id IN (SELECT Id FROM @FilteredBookIds))
                            AND (@AuthorName IS NULL OR a.Name LIKE '%' + @AuthorName + '%')
                            AND (@GenreName IS NULL OR g.Name LIKE '%' + @GenreName + '%')
                            AND (@MinPrice IS NULL OR b.Price >= @MinPrice)
                            AND (@MaxPrice IS NULL OR b.Price <= @MaxPrice)
                        GROUP BY b.Id, b.Title, b.Price
                        HAVING (@MinAverageRating IS NULL OR COALESCE(AVG(CAST(r.Rating AS FLOAT)), 0) >= @MinAverageRating)
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
                        fb.Price,
                        COALESCE(ba.AuthorNames, '') AS AuthorNames,
                        COALESCE(bg.GenreNames, '') AS GenreNames,
                        fb.AverageRating
                    FROM FilteredBooks fb
                    LEFT JOIN BookAuthors ba ON fb.Id = ba.Id
                    LEFT JOIN BookGenres bg ON fb.Id = bg.Id
                    ORDER BY fb.AverageRating DESC, fb.Title;
                END
            ");
    }
}
