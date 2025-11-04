using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController(IReviewService reviewService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetAll()
        {
            var result = await reviewService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewResponse>> GetById(int id)
        {
            var result = await reviewService.GetByIdAsync(id);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ReviewResponse>> Create(ReviewCreateRequest reviewCreate)
        {
            try
            {
                var result = await reviewService.CreateAsync(reviewCreate);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ReviewUpdateRequest reviewUpdate)
        {
            var result = await reviewService.UpdateAsync(id, reviewUpdate);
            return result is null
                ? NotFound()
                : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await reviewService.DeleteAsync(id)
                ? NoContent()
                : NotFound();
        }
    }
}
