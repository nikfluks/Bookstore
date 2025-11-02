using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorsController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorResponse>>> GetAll()
        {
            var result = await _authorService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorResponse>> GetById(int id)
        {
            var result = await _authorService.GetByIdAsync(id);
            return result is null
                ? BadRequest()
                : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<AuthorResponse>> Create(AuthorCreateRequest authorCreate)
        {
            var result = await _authorService.CreateAsync(authorCreate);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, AuthorUpdateRequest authorUpdate)
        {
            if (id != authorUpdate.Id)
            {
                return BadRequest();
            }

            var result = await _authorService.UpdateAsync(authorUpdate);
            return result is null
                ? BadRequest()
                : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await _authorService.DeleteAsync(id)
                ? NoContent()
                : BadRequest();
        }
    }
}
