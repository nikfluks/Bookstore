using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Application.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GenresController(IGenreService genreService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
        public async Task<ActionResult<IEnumerable<GenreResponse>>> GetAll()
        {
            var result = await genreService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
        public async Task<ActionResult<GenreResponse>> GetById(int id)
        {
            var result = await genreService.GetByIdAsync(id);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = Roles.ReadWrite)]
        public async Task<ActionResult<GenreResponse>> Create(GenreCreateRequest genreCreate)
        {
            var result = await genreService.CreateAsync(genreCreate);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.ReadWrite)]
        public async Task<IActionResult> Update(int id, GenreUpdateRequest genreUpdate)
        {
            var result = await genreService.UpdateAsync(id, genreUpdate);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.ReadWrite)]
        public async Task<IActionResult> Delete(int id)
        {
            return await genreService.DeleteAsync(id)
                ? NoContent()
                : NotFound();
        }
    }
}
