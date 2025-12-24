using Bookstore.Application.Constants;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BooksController(IBookService bookService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetAll()
        {
            var result = await bookService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("details")]
        [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
        public async Task<ActionResult<IEnumerable<BookDetailedResponse>>> GetAllDetailed()
        {
            var result = await bookService.GetAllDetailedAsync();
            return Ok(result);
        }

        [HttpGet("top-10")]
        [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
        public async Task<ActionResult<IEnumerable<BookDetailedResponse>>> GetTop10ByRating()
        {
            var result = await bookService.GetTop10ByRatingAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
        public async Task<ActionResult<BookResponse>> GetById(int id)
        {
            var result = await bookService.GetByIdAsync(id);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpGet("search")]
        [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
        public async Task<ActionResult<IEnumerable<BookDetailedResponse>>> Search([FromQuery] BookSearchRequest request)
        {
            var result = await bookService.SearchBooksAsync(request);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = Roles.ReadWrite)]
        public async Task<ActionResult<BookDetailedResponse>> Create(BookCreateRequest bookCreate)
        {
            var result = await bookService.CreateAsync(bookCreate);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.ReadWrite)]
        public async Task<ActionResult<BookResponse>> Update(int id, BookPriceUpdateRequest priceUpdate)
        {
            var result = await bookService.UpdateAsync(id, priceUpdate);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.ReadWrite)]
        public async Task<IActionResult> Delete(int id)
        {
            return await bookService.DeleteAsync(id)
                ? NoContent()
                : NotFound();
        }

        [HttpPut("{id}/authors")]
        [Authorize(Roles = Roles.ReadWrite)]
        public async Task<ActionResult<BookDetailedResponse>> UpdateAuthors(int id, BookAuthorsUpdateRequest authorsUpdate)
        {
            var result = await bookService.UpdateAuthorsAsync(id, authorsUpdate);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpPut("{id}/genres")]
        [Authorize(Roles = Roles.ReadWrite)]
        public async Task<ActionResult<BookDetailedResponse>> UpdateGenres(int id, BookGenresUpdateRequest genresUpdate)
        {
            var result = await bookService.UpdateGenresAsync(id, genresUpdate);
            return result is null
                ? NotFound()
                : Ok(result);
        }
    }
}
