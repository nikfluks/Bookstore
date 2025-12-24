using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookstore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix column lengths to allow indexing (nvarchar(max) cannot be indexed)
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Authors",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Genres",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Books",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_Reviews_BookId 
                ON Reviews(BookId ASC)
                INCLUDE (Rating)
                WITH (DROP_EXISTING = ON);
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_Books_Price 
                ON Books(Price ASC)
                INCLUDE (Title)
                WITH (DROP_EXISTING = ON);
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_Authors_Name 
                ON Authors(Name ASC)
                WITH (DROP_EXISTING = ON);
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_Genres_Name 
                ON Genres(Name ASC)
                WITH (DROP_EXISTING = ON);
            ");

            // Create full-text catalog (must run outside transaction)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'BookstoreCatalog')
                BEGIN
                    CREATE FULLTEXT CATALOG BookstoreCatalog AS DEFAULT;
                END
            ", suppressTransaction: true);

            // Create full-text index on Books.Title (must run outside transaction)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Books'))
                BEGIN
                    CREATE FULLTEXT INDEX ON Books(Title LANGUAGE 1033) -- 1033 = English
                    KEY INDEX PK_Books
                    ON BookstoreCatalog
                    WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM);
                END
            ", suppressTransaction: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Books'))
                BEGIN
                    DROP FULLTEXT INDEX ON Books;
                END
            ", suppressTransaction: true);

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'BookstoreCatalog')
                BEGIN
                    DROP FULLTEXT CATALOG BookstoreCatalog;
                END
            ", suppressTransaction: true);

            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Genres_Name ON Genres;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Authors_Name ON Authors;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Books_Price ON Books;");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_Reviews_BookId 
                ON Reviews(BookId ASC)
                WITH (DROP_EXISTING = ON);
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Genres",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Authors",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
