using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.DTOs;
using proiect_licenta.Models;

namespace proiect_licenta.Services;

public class LibraryService
{
    // add  access checking
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    //private readonly PrivilegeChecker _privilegeChecker;
    //private readonly ClaimsPrincipal _user;

    public LibraryService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<LibraryRecord>> GetAllLibs()
    {
        var libraryRecords = await _context.Libraries
            .ToListAsync();
        return libraryRecords;
    }
    
    public async Task<IEnumerable<LibraryRecord>> GetAllLibsByUserId(){
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var libraryRecords = await _context.Libraries
            .Where(libraryRecord => libraryRecord.UserId == userId)
            .Include(i => i.App)
            .ToListAsync();
            
        if (libraryRecords.Count == 0)
            throw new Exception("No installed apps found for this user.");
        
        return libraryRecords;
    }
    
    public async Task<LibraryDto> CreateLibraryRecord(LibraryDto libraryRecord)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User not found.");
        
        var app = await _context.Apps.FindAsync(libraryRecord.AppId);
        if (app == null)
            throw new Exception("App not found.");
        
        var paymentRecord = await _context.PaymentRecords.FindAsync(libraryRecord.PaymentId);
        if (paymentRecord == null)
            throw new Exception("Payment not found.");

        LibraryRecord newLibraryRecord = new LibraryRecord
        {
            AppId = libraryRecord.AppId,
            PaymentId = libraryRecord.PaymentId,
            UserId = userId,
            User = user,
            App = app,
            PaymentRecord = paymentRecord,
        };

        _context.Libraries.Add(newLibraryRecord);

        await _context.SaveChangesAsync();
        return libraryRecord;
    }

    /* Unnecessary?
    public async Task<Install> EditInstall(Install install)
    {
        //check privilege
        var existingInstall = await _context.Installs.FindAsync(install.Id);
        if (existingInstall == null)
            throw new Exception("Install does not exist");

        _context.Entry(install).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return install;
    }
    */

    public async Task DeleteLibraryRecord(int id)
    {
        var libraryRecord = await _context.Libraries.FindAsync(id);
        if (libraryRecord == null)
            throw new Exception("Install does not exist");

        _context.Libraries.Remove(libraryRecord);
        await _context.SaveChangesAsync();
    }
}