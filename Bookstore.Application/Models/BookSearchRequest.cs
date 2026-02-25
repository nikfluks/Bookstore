using System.ComponentModel.DataAnnotations;
using Bookstore.Application.Constants;

namespace Bookstore.Application.Models;

public record BookSearchRequest
(
    string? BookTitle = null,
    string? AuthorName = null,
    string? GenreName = null,
    float? MinPrice = null,
    float? MaxPrice = null,
    float? MinAverageRating = null,
    [Range(1, int.MaxValue)]
    int PageNumber = 1,
    [Range(1, Pagination.MaxPageSize)]
    int PageSize = Pagination.DefaultPageSize
) : PagedRequest(PageNumber, PageSize);
