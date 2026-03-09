using Asp.Versioning;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Application.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers;

[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class GenresController(IGenreService genreService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    public async Task<ActionResult<IEnumerable<GenreResponse>>> GetAllAsync()
    {
        var result = await genreService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    public async Task<ActionResult<GenreResponse>> GetByIdAsync(int id)
    {
        var result = await genreService.GetByIdAsync(id);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<GenreResponse>> CreateAsync(GenreCreateRequest genreCreate)
    {
        var result = await genreService.CreateAsync(genreCreate);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<GenreResponse>> UpdateAsync(int id, GenreUpdateRequest genreUpdate)
    {
        var result = await genreService.UpdateAsync(id, genreUpdate);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        return await genreService.DeleteAsync(id)
            ? NoContent()
            : NotFound();
    }
}
