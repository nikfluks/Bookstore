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
public class ReviewsController(IReviewService reviewService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetAll()
    {
        var result = await reviewService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{Roles.Read},{Roles.ReadWrite}")]
    public async Task<ActionResult<ReviewResponse>> GetById(int id)
    {
        var result = await reviewService.GetByIdAsync(id);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<ReviewResponse>> Create(ReviewCreateRequest reviewCreate)
    {
        var result = await reviewService.CreateAsync(reviewCreate);

        if (!result.IsSuccessful)
        {
            return Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request");
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Review!.Id }, result.Review);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<ReviewResponse>> Update(int id, ReviewUpdateRequest reviewUpdate)
    {
        var result = await reviewService.UpdateAsync(id, reviewUpdate);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<IActionResult> Delete(int id)
    {
        return await reviewService.DeleteAsync(id)
            ? NoContent()
            : NotFound();
    }
}
