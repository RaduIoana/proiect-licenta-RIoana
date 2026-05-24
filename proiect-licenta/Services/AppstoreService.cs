using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.DTOs;
using proiect_licenta.Models;

namespace proiect_licenta.Services;

public class AppstoreService
{
    private readonly ApplicationDbContext _context;
    private readonly AppService _appService;
    private readonly FileService _fileService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppstoreService(ApplicationDbContext context, AppService appService,
        FileService fileService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _appService = appService;
        _fileService = fileService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<(Stream stream, string fileName)> InstallApp(int appId)
    {
        /*
         * - move checking for subscription to hardhat, send something else with the transaction to check if disc applies
         */
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.Include(u => u.Libraries)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new Exception("User Not Found");

        var app = await _context.Apps.Include(a => a.AppFile)
            .FirstOrDefaultAsync(a => a.Id == appId);
        if (app == null) 
            throw new Exception("App not found.");
        if (app.AppFile == null)
            throw new Exception("App has no executable uploaded.");

        if(!await _appService.AppOwned(app.Id))
            throw new Exception("App is not owned.");

        return await _fileService.DownloadFileFromIpfs(app.AppFile.Cid);
    }

    // this should mostly have the effect of removing from library
    // also shouldn't work for paid apps? idk
    public async Task RemoveFromLibrary(int appId)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.Include(u => u.Libraries)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new Exception("User Not Found");

        if (!await _appService.AppOwned(appId))
            throw new Exception("App not owned.");

        var appToRemove = user.Libraries.FirstOrDefault(i => i.AppId == appId);
        user.Libraries.Remove(appToRemove);
        _context.SaveChangesAsync();

        Console.WriteLine("The app has been successfully uninstalled.");
    }

    public async Task<decimal> CalculateAppPrice(int appId)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.Include(u => u.Libraries)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new Exception("User Not Found");

        var app = await _context.Apps.Include(a => a.AppFile)
            .FirstOrDefaultAsync(a => a.Id == appId);
        if (app == null) 
            throw new Exception("App not found.");

        var defaultPrice = app.Price;
        
        // add voucher check

        return defaultPrice;
    }
}