using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController(IBookService bookService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDetailedResponse>>> GetAllDetailed()
        {
            var result = await bookService.GetAllDetailedAsync();
            return Ok(result);
        }

        [HttpGet("top-10")]
        public async Task<ActionResult<IEnumerable<BookDetailedResponse>>> GetTop10ByRating()
        {
            var result = await bookService.GetTop10ByRatingAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDetailedResponse>> GetById(int id)
        {
            var result = await bookService.GetByIdAsync(id);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<BookDetailedResponse>> Create(BookCreateRequest bookCreate)
        {
            var result = await bookService.CreateAsync(bookCreate);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BookPriceUpdateRequest priceUpdate)
        {
            var result = await bookService.UpdateAsync(id, priceUpdate);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await bookService.DeleteAsync(id)
                ? NoContent()
                : NotFound();
        }

        [HttpPut("{id}/authors")]
        public async Task<IActionResult> UpdateAuthors(int id, BookAuthorsUpdateRequest authorsUpdate)
        {
            var result = await bookService.UpdateAuthorsAsync(id, authorsUpdate);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpPut("{id}/genres")]
        public async Task<IActionResult> UpdateGenres(int id, BookGenresUpdateRequest genresUpdate)
        {
            var result = await bookService.UpdateGenresAsync(id, genresUpdate);
            return result is null
                ? NotFound()
                : Ok(result);
        }
    }
}
