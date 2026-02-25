using System.ComponentModel.DataAnnotations;
using Bookstore.Application.Constants;

namespace Bookstore.Application.Models;

public record PagedRequest
(
    [Range(1, int.MaxValue)]
    int PageNumber = 1,
    [Range(1, Pagination.MaxPageSize)]
    int PageSize = Pagination.DefaultPageSize
);
