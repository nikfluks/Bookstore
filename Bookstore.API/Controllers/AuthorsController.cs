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
    public class AuthorsController(IAuthorService authorService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
        public async Task<ActionResult<IEnumerable<AuthorResponse>>> GetAll()
        {
            var result = await authorService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
        public async Task<ActionResult<AuthorResponse>> GetById(int id)
        {
            var result = await authorService.GetByIdAsync(id);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = Roles.ReadWrite)]
        public async Task<ActionResult<AuthorResponse>> Create(AuthorCreateRequest authorCreate)
        {
            var result = await authorService.CreateAsync(authorCreate);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.ReadWrite)]
        public async Task<ActionResult<AuthorResponse>> Update(int id, AuthorUpdateRequest authorUpdate)
        {
            var result = await authorService.UpdateAsync(id, authorUpdate);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.ReadWrite)]
        public async Task<IActionResult> Delete(int id)
        {
            return await authorService.DeleteAsync(id)
                ? NoContent()
                : NotFound();
        }
    }
}
