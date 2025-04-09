using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using System.Security.Claims;

namespace proiect_licenta.Services
{
    public class ReportService
    {
        // add  access checking
        private readonly ApplicationDbContext _context;
        //private readonly PrivilegeChecker _privilegeChecker;
        //private readonly ClaimsPrincipal _user;

        public ReportService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
        }

        public async Task<IEnumerable<Report>> GetAllReports()
        {
            var reports = await _context.Reports
                .ToListAsync();
            return reports;
        }

        public async Task<Report> GetReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) { throw new Exception(); }
            return report;
        }

        public async Task<Report> CreateReport(Report report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }
/*
        public async Task<Report> EditReport(Report report)
        {
            //idk
            //check privilege

            var existingReport = await _context.Reports.FindAsync(report.Id);
            if (existingReport == null)
                throw new Exception("Report does not exist");

            _context.Entry(report).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return report;
        }
*/
        public async Task DeleteReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                throw new Exception("Report does not exist");

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
        }
    }
}
