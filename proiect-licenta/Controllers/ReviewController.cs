using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers
{
    [Route("api/Reviews")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _reviewService;

        public ReviewController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET: api/MyModel
        [HttpGet("get_all_reviews")]
        public async Task<ActionResult<IEnumerable<Review>>> GetAllReviews()
        {
            // Simulate async database access
            return Ok(await _reviewService.GetAllReviews());
        }

        // GET: api/MyModel/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetById(int id)
        {
            return Ok(await _reviewService.GetReview(id));
        }

        [HttpPut]
        public async Task<ActionResult<Review>> PutReview(Review review)
        {
            return Ok(await _reviewService.EditReview(review));
        }

        [HttpPost]
        public async Task<ActionResult<Review>> PostReview(Review review)
        {
            return Ok(await _reviewService.CreateReview(review));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReview(int id)
        {
            await _reviewService.DeleteReview(id);
            return NoContent();
        }
    }

}
