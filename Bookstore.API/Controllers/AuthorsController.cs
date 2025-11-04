using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController(IAuthorService authorService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorResponse>>> GetAll()
        {
            var result = await authorService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorResponse>> GetById(int id)
        {
            var result = await authorService.GetByIdAsync(id);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<AuthorResponse>> Create(AuthorCreateRequest authorCreate)
        {
            var result = await authorService.CreateAsync(authorCreate);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, AuthorUpdateRequest authorUpdate)
        {
            var result = await authorService.UpdateAsync(id, authorUpdate);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await authorService.DeleteAsync(id)
                ? NoContent()
                : NotFound();
        }
    }
}
