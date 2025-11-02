using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly IGenreService _genreService;

        public GenresController(IGenreService genreService)
        {
            _genreService = genreService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenreResponse>>> GetAll()
        {
            var result = await _genreService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GenreResponse>> GetById(int id)
        {
            var result = await _genreService.GetByIdAsync(id);
            return result is null
                ? BadRequest()
                : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<GenreResponse>> Create(GenreCreateRequest genreCreate)
        {
            var result = await _genreService.CreateAsync(genreCreate);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, GenreUpdateRequest genreUpdate)
        {
            var result = await _genreService.UpdateAsync(id, genreUpdate);
            return result is null
                ? BadRequest()
                : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await _genreService.DeleteAsync(id)
                ? NoContent()
                : BadRequest();
        }
    }
}
