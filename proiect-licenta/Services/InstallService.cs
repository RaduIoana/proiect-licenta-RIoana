using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;

namespace proiect_licenta.Services;

public class InstallService
{
    // add  access checking
    private readonly ApplicationDbContext _context;
    //private readonly PrivilegeChecker _privilegeChecker;
    //private readonly ClaimsPrincipal _user;

    public InstallService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
    }

    public async Task<IEnumerable<Install>> GetAllInstalls()
    {
        var installs = await _context.Installs
            .ToListAsync();
        return installs;
    }

    /* Unnecessary?
    public async Task<Install> GetInstall(int id)
    {
        var install = await _context.Installs.FindAsync(id);
        if (install == null) { throw new Exception(); }
        return install;
    }
    */
    
    public async Task<Install> CreateInstall(Install install)
    {
        _context.Installs.Add(install);
        await _context.SaveChangesAsync();
        return install;
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

    public async Task DeleteInstall(int id)
    {
        var task = await _context.Installs.FindAsync(id);
        if (task == null)
            throw new Exception("Install does not exist");

        _context.Installs.Remove(task);
        await _context.SaveChangesAsync();
    }
}