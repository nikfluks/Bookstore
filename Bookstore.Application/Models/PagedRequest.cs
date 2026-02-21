using Bookstore.Application.Constants;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Models;

public record PagedRequest(
    [Range(1, int.MaxValue)]
    int PageNumber = 1,
    [Range(1, Pagination.MaxPageSize)]
    int PageSize = Pagination.DefaultPageSize
);
