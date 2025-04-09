using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using System.Security.Claims;

namespace proiect_licenta.Services
{
    public class ReviewService
    {
        // add  access checking
        private readonly ApplicationDbContext _context;
        //private readonly PrivilegeChecker _privilegeChecker;
        //private readonly ClaimsPrincipal _user;

        public ReviewService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetAllReviews()
        {
            var reviews = await _context.Reviews
                .ToListAsync();
            return reviews;
        }

        public async Task<Review> GetReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) { throw new Exception(); }
            return review;
        }

        public async Task<Review> CreateReview(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<Review> EditReview(Review review)
        {
            //idk
            //check privilege
            var existingReview = await _context.Reviews.FindAsync(review.UserId);
            if (existingReview == null)
                throw new Exception("Review does not exist");

            _context.Entry(review).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                throw new Exception("Review does not exist");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }
}
