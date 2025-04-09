using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using System.Security.Claims;

namespace proiect_licenta.Services
{
    public class AppService
    {
        // add  access checking
        private readonly ApplicationDbContext _context;
        //private readonly PrivilegeChecker _privilegeChecker;
        //private readonly ClaimsPrincipal _user;

        public AppService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
        }

        public async Task<IEnumerable<App>> GetAllApps()
        {
            var apps = await _context.Apps
                .ToListAsync();
            return apps;
        }

        public async Task<App> GetApp(int id)
        {
            var app = await _context.Apps.FindAsync(id);
            if (app == null) { throw new Exception(); }
            return app;
        }

        public async Task<App> CreateApp(App app)
        {
            _context.Apps.Add(app);
            await _context.SaveChangesAsync();
            return app;
        }

        public async Task<App> EditApp(App app)
        {
            //check privilege
            var existingApp = await _context.Apps.FindAsync(app.Id);
            if (existingApp == null)
                throw new Exception("App does not exist");

            _context.Entry(app).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return app;
        }

        public async Task DeleteApp(int id)
        {
            var task = await _context.Apps.FindAsync(id);
            if (task == null)
                throw new Exception("App does not exist");

            _context.Apps.Remove(task);
            await _context.SaveChangesAsync();
        }

        public double GetPriceWithDiscount(int appId)
        {
            var app = _context.Apps.Find(appId);
            if (app == null)
                throw new Exception("App does not exist");

            return app.Price - (app.Price * app.Discount / 100);
        }

        public double GetPriceWithoutDiscount(int appId)
        {
            var app = _context.Apps.Find(appId);
            if (app == null)
                throw new Exception("App does not exist");

            return app.Price;
        }
    }
}
