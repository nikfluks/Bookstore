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
public class ReviewsController(IReviewService reviewService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetAllAsync()
    {
        var result = await reviewService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    public async Task<ActionResult<ReviewResponse>> GetByIdAsync(int id)
    {
        var result = await reviewService.GetByIdAsync(id);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<ReviewResponse>> CreateAsync(ReviewCreateRequest reviewCreate)
    {
        try
        {
            var result = await reviewService.CreateAsync(reviewCreate);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<ReviewResponse>> UpdateAsync(int id, ReviewUpdateRequest reviewUpdate)
    {
        var result = await reviewService.UpdateAsync(id, reviewUpdate);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        return await reviewService.DeleteAsync(id)
            ? NoContent()
            : NotFound();
    }
}
