using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using System.Security.Claims;

namespace proiect_licenta.Services
{
    public class VoucherService
    {
        // add  access checking
        private readonly ApplicationDbContext _context;
        //private readonly PrivilegeChecker _privilegeChecker;
        //private readonly ClaimsPrincipal _user;

        public VoucherService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
        }

        public async Task<IEnumerable<Voucher>> GetAllVouchers()
        {
            var vouchers = await _context.Vouchers
                .ToListAsync();
            return vouchers;
        }

        public async Task<Voucher> GetVoucher(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) { throw new Exception(); }
            return voucher;
        }

        public async Task<Voucher> CreateVoucher(Voucher voucher)
        {
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();
            return voucher;
        }
/*
        public async Task<Voucher> EditVoucher(Voucher voucher)
        {
            //check privilege
            var existingVoucher = await _context.Vouchers.FindAsync(voucher.Id);
            if (existingVoucher == null)
                throw new Exception("Voucher does not exist");

            _context.Entry(voucher).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return voucher;
        }
*/
        public async Task DeleteVoucher(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
                throw new Exception("Voucher does not exist");

            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();
        }
    }
}
