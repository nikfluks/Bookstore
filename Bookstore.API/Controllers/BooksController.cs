using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetAll()
        {
            var result = await _bookService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookResponse>> GetById(int id)
        {
            var result = await _bookService.GetByIdAsync(id);
            return result is null
                ? BadRequest()
                : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<BookResponse>> Create(BookCreateRequest bookCreate)
        {
            var result = await _bookService.CreateAsync(bookCreate);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BookUpdateRequest bookUpdate)
        {
            if (id != bookUpdate.Id)
            {
                return BadRequest();
            }

            var result = await _bookService.UpdateAsync(bookUpdate);
            return result is null
                ? BadRequest()
                : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await _bookService.DeleteAsync(id)
                ? NoContent()
                : BadRequest();
        }
    }
}
