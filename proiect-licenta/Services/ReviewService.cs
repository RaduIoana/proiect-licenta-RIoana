using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using proiect_licenta.DTOs;
using proiect_licenta.Exceptions;

namespace proiect_licenta.Services;

public class ReviewService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<MyUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ReviewService(ApplicationDbContext context, UserManager<MyUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetAllReviews()
    {
        var reviews = await _context.Reviews.Include(r => r.User)
            .ToListAsync();
        var reviewReturns = new List<ReviewResponseDto>();

        foreach (var review in reviews)
        {
            reviewReturns.Add(new ReviewResponseDto
            {
                AppId = review.AppId,
                Username = review.User.UserName,
                Rating = review.Rating,
                Title = review.Title,
                Content = review.Content,
                PostDate = review.PostDate,
                EditDate = review.EditDate
            });
        }
            
        return reviewReturns;
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByAppId(int appId)
    {
        var reviews = await _context.Reviews.Where(r => r.AppId == appId)
            .Include(r => r.User).ToListAsync();
        var reviewReturns = new List<ReviewResponseDto>();
            
        foreach (var review in reviews)
        {
            reviewReturns.Add(new ReviewResponseDto
            {
                AppId = review.AppId,
                Username = review.User.UserName,
                Rating = review.Rating,
                Title = review.Title,
                Content = review.Content,
                PostDate = review.PostDate,
                EditDate = review.EditDate
            });
        }
        
        if (reviewReturns.Count == 0)
            throw new NotFoundException("No reviews found.");

        return reviewReturns;
    }

    public async Task<ReviewResponseDto> GetOwnReview(int appId)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new UnauthorizedException("Unauthorized");
            
        var review = await _context.Reviews.Where(r => r.AppId == appId && r.UserId == userId)
            .Include(r => r.User).FirstOrDefaultAsync();
        if (review == null)
            throw new NotFoundException("Review not found");
        return new ReviewResponseDto
        {
            AppId = review.AppId,
            Username = review.User.UserName,
            Rating = review.Rating,
            Title = review.Title,
            Content = review.Content,
            PostDate = review.PostDate,
            EditDate = review.EditDate
        };
    }
        
    public async Task<Boolean> ReviewExists(int appId)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;
            
        var review = await _context.Reviews.Where(r => r.AppId == appId && r.UserId == userId).FirstOrDefaultAsync();
        if (review == null)
        {
            return false;
        }
        return true;
    }

    public async Task<ReviewResponseDto> CreateReview(ReviewRequestDto reviewRequest, int appId)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new UnauthorizedException("Unauthorized");

        // fix ef usermanager bug
        _context.Attach(user);
        var review = new Review
        {
            AppId = appId,
            UserId = user.Id,
            Rating = reviewRequest.Rating,
            PostDate = DateTime.Now,
            Title = reviewRequest.Title,
            Content = reviewRequest.Content,
        };
            
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return new ReviewResponseDto
        {
            Content = review.Content,
            Username = user.UserName,
            Rating = review.Rating,
            PostDate = review.PostDate,
            Title = review.Title,
            AppId = review.AppId
        };
    }

    public async Task<ReviewResponseDto> EditReview(ReviewRequestDto reviewRequest, int appId)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new UnauthorizedException("Unauthorized");
            
        var existingReview = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.AppId == appId && r.UserId == userId)
            .FirstOrDefaultAsync();
        if (existingReview == null)
            throw new NotFoundException("Review not found");
            
        if (existingReview.UserId != userId || !_userManager.GetRolesAsync(user).Result.Contains("ADMIN"))
            throw new ForbiddenException("Forbidden");

        existingReview.Rating = reviewRequest.Rating;
        existingReview.Title = reviewRequest.Title;
        existingReview.Content = reviewRequest.Content;
        existingReview.EditDate = DateTime.Now;
        _context.Entry(existingReview).State = EntityState.Modified;
        await _context.SaveChangesAsync();
            
        return new ReviewResponseDto
        {
            Content = existingReview.Content,
            Username = existingReview.User.UserName,
            Rating = existingReview.Rating,
            PostDate = existingReview.PostDate,
            Title = existingReview.Title,
            AppId = existingReview.AppId
        };
    }

    public async Task DeleteReview(int appId)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new UnauthorizedException("Unauthorized");
            
        var review = await _context.Reviews.Where(r => r.AppId == appId && r.UserId == userId).FirstOrDefaultAsync();
        if (review == null)
            throw new NotFoundException("Review not found");
            
        if (review.UserId != userId || !_userManager.GetRolesAsync(user).Result.Contains("ADMIN"))
            throw new ForbiddenException("Forbidden");

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }
}