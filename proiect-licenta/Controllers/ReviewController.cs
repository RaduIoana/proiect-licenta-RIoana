using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Contexts;
using proiect_licenta.DTOs;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

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

    [AllowAnonymous]
    [HttpGet("get_all_reviews")]
    public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetAllReviews()
    {
        return Ok(await _reviewService.GetAllReviews());
    }

    [AllowAnonymous]
    [HttpGet("by_app/{appId}")]
    public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetReviewsByAppId(int appId)
    {
        return Ok(await _reviewService.GetReviewsByAppId(appId));
    }

    [HttpGet("own/{appId}")]
    public async Task<ActionResult<ReviewResponseDto>> GetOwnReviewByAppId(int appId)
    {
        return Ok(await _reviewService.GetOwnReview(appId));
    }
    
    [AllowAnonymous]
    [HttpGet("exists/{appId}")]
    public async Task<Boolean> ReviewExists(int appId)
    {
        return await _reviewService.ReviewExists(appId);
    }

    [HttpPut("{appId}")]
    public async Task<ActionResult<ReviewResponseDto>> PutReview(ReviewRequestDto review, int appId)
    {
        return Ok(await _reviewService.EditReview(review, appId));
    }

    [HttpPost("{appId}")]
    public async Task<ActionResult<ReviewResponseDto>> PostReview(ReviewRequestDto review, int appId)
    {
        return Ok(await _reviewService.CreateReview(review, appId));
    }
    
    [HttpDelete("{appId}")]
    public async Task<ActionResult> DeleteReview(int appId)
    {
        await _reviewService.DeleteReview(appId);
        return NoContent();
    }
}