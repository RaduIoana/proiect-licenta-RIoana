using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using System.Security.Claims;

namespace proiect_licenta.Services
{
    public class RefundService
    {
        // add  access checking
        private readonly ApplicationDbContext _context;
        //private readonly PrivilegeChecker _privilegeChecker;
        //private readonly ClaimsPrincipal _user;

        public RefundService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
        }

        public async Task<IEnumerable<RefundRequest>> GetAllRefunds()
        {
            var refunds = await _context.RefundRequests
                .ToListAsync();
            return refunds;
        }

        public async Task<RefundRequest> GetRefund(int id)
        {
            var refund = await _context.RefundRequests.FindAsync(id);
            if (refund == null) { throw new Exception(); }
            return refund;
        }

        public async Task<RefundRequest> CreateRefund(RefundRequest refund)
        {
            _context.RefundRequests.Add(refund);
            await _context.SaveChangesAsync();
            return refund;
        }

        public async Task<RefundRequest> EditRefund(RefundRequest refund)
        {
            //check privilege
            var existingRefund = await _context.RefundRequests.FindAsync(refund.Id);
            if (existingRefund == null)
                throw new Exception("Refund request does not exist");

            _context.Entry(refund).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return refund;
        }

        public async Task DeleteRefund(int id)
        {
            var refund = await _context.RefundRequests.FindAsync(id);
            if (refund == null)
                throw new Exception("Refund request does not exist");

            _context.RefundRequests.Remove(refund);
            await _context.SaveChangesAsync();
        }
    }
}
