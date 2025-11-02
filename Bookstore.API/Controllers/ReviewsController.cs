using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetAll()
        {
            var result = await _reviewService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewResponse>> GetById(int id)
        {
            var result = await _reviewService.GetByIdAsync(id);
            return result is null
                ? BadRequest()
                : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ReviewResponse>> Create(ReviewCreateRequest reviewCreate)
        {
            try
            {
                var result = await _reviewService.CreateAsync(reviewCreate);
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
            if (id != reviewUpdate.Id)
            {
                return BadRequest();
            }

            var result = await _reviewService.UpdateAsync(reviewUpdate);
            return result is null
                ? BadRequest()
                : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await _reviewService.DeleteAsync(id)
                ? NoContent()
                : BadRequest();
        }
    }
}
