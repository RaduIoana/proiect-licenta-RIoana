using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using System.Security.Claims;

namespace proiect_licenta.Services
{
    public class PayRecordService
    {
        // add  access checking
        private readonly ApplicationDbContext _context;
        //private readonly PrivilegeChecker _privilegeChecker;
        //private readonly ClaimsPrincipal _user;

        public PayRecordService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentRecord>> GetAllPaymentRecords()
        {
            var payRecords = await _context.PaymentRecords
                .ToListAsync();
            return payRecords;
        }

        public async Task<PaymentRecord> GetPaymentRecord(int id)
        {
            var payRecord = await _context.PaymentRecords.FindAsync(id);
            if (payRecord == null) { throw new Exception(); }
            return payRecord;
        }

        public async Task<PaymentRecord> CreatePaymentRecord(PaymentRecord payRecord)
        {
            _context.PaymentRecords.Add(payRecord);
            await _context.SaveChangesAsync();
            return payRecord;
        }

        public async Task<PaymentRecord> EditPaymentRecord(PaymentRecord payRecord)
        {
            //check privilege
            var existingPaymentRecord = await _context.PaymentRecords.FindAsync(payRecord.Id);
            if (existingPaymentRecord == null)
                throw new Exception("PaymentRecord does not exist");

            _context.Entry(payRecord).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return payRecord;
        }

        public async Task DeletePaymentRecord(int id)
        {
            var task = await _context.PaymentRecords.FindAsync(id);
            if (task == null)
                throw new Exception("PaymentRecord does not exist");

            _context.PaymentRecords.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}
