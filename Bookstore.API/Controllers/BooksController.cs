using Asp.Versioning;
using Bookstore.Application.Constants;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Swashbuckle.AspNetCore.Annotations;

namespace Bookstore.API.Controllers;

[ApiVersion(1.0)]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class BooksController(IBookService bookService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    public async Task<ActionResult<PagedResponse<BookResponse>>> GetAllAsync([FromQuery] PagedRequest request)
    {
        var result = await bookService.GetAllAsync(request);
        return Ok(result);
    }

    [MapToApiVersion(2.0)]
    [HttpGet("odata")]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    [EnableQuery(PageSize = Pagination.DefaultPageSize)]
    [SwaggerOperation(
        Description = "Example: /api/books/odata?$filter=price lt 100&$orderby=title desc&$top=10&$skip=0" +
            "&$count=true&$select=id,title,price&$expand=authors($select=name),reviews($filter=rating ge 4),genres")]
    public IQueryable<Book> GetBooksOData(
        [FromQuery(Name = "$filter")] string? filter = null,
        [FromQuery(Name = "$orderby")] string? orderby = null,
        [FromQuery(Name = "$top")] int? top = null,
        [FromQuery(Name = "$skip")] int? skip = null,
        [FromQuery(Name = "$count")] bool? count = null,
        [FromQuery(Name = "$select")] string? select = null,
        [FromQuery(Name = "$expand")] string? expand = null)
    {
        return bookService.GetBooksQueryable();
    }

    [HttpGet("details")]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    public async Task<ActionResult<PagedResponse<BookDetailedResponse>>> GetAllDetailedAsync([FromQuery] PagedRequest request)
    {
        var result = await bookService.GetAllDetailedAsync(request);
        return Ok(result);
    }

    [HttpGet("top-10")]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    [SwaggerOperation(Summary = "Gets top 10 books by average rating")]
    public async Task<ActionResult<IEnumerable<BookDetailedResponse>>> GetTop10ByRatingAsync()
    {
        var result = await bookService.GetTop10ByRatingAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    public async Task<ActionResult<BookResponse>> GetByIdAsync(int id)
    {
        var result = await bookService.GetByIdAsync(id);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [MapToApiVersion(2.0)]
    [HttpGet("search")]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    public async Task<ActionResult<PagedResponse<BookDetailedResponse>>> SearchAsync([FromQuery] BookSearchRequest request)
    {
        var result = await bookService.SearchBooksAsync(request);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<BookDetailedResponse>> CreateAsync(BookCreateRequest bookCreate)
    {
        var result = await bookService.CreateAsync(bookCreate);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<BookResponse>> UpdateAsync(int id, BookPriceUpdateRequest priceUpdate)
    {
        var result = await bookService.UpdateAsync(id, priceUpdate);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        return await bookService.DeleteAsync(id)
            ? NoContent()
            : NotFound();
    }

    [HttpPut("{id}/authors")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<BookDetailedResponse>> UpdateAuthorsAsync(int id, BookAuthorsUpdateRequest authorsUpdate)
    {
        var result = await bookService.UpdateAuthorsAsync(id, authorsUpdate);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPut("{id}/genres")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<BookDetailedResponse>> UpdateGenresAsync(int id, BookGenresUpdateRequest genresUpdate)
    {
        var result = await bookService.UpdateGenresAsync(id, genresUpdate);
        return result is null
            ? NotFound()
            : Ok(result);
    }
}
