using Asp.Versioning;
using Bookstore.API.Constants;
using Bookstore.Application.Constants;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Bookstore.API.Controllers;

[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitConstants.AuthenticatedPolicyName)]
public class AuthorsController(IAuthorService authorService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    public async Task<ActionResult<IEnumerable<AuthorResponse>>> GetAllAsync()
    {
        var result = await authorService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    public async Task<ActionResult<AuthorResponse>> GetByIdAsync(int id)
    {
        var result = await authorService.GetByIdAsync(id);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<AuthorResponse>> CreateAsync(AuthorCreateRequest authorCreate)
    {
        var result = await authorService.CreateAsync(authorCreate);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<AuthorResponse>> UpdateAsync(int id, AuthorUpdateRequest authorUpdate)
    {
        var result = await authorService.UpdateAsync(id, authorUpdate);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        return await authorService.DeleteAsync(id)
            ? NoContent()
            : NotFound();
    }
}
