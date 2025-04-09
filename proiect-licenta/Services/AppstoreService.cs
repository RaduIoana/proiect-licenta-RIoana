using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;

namespace proiect_licenta.Services
{
    public class AppstoreService
    {
        // add  access checking
        private readonly ApplicationDbContext _context;
        private readonly AppService _appService;
        private readonly InstallService _installService;
        //private readonly PrivilegeChecker _privilegeChecker;
        //private readonly ClaimsPrincipal _user;

        public AppstoreService(ApplicationDbContext context, AppService appService, 
            InstallService installService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _appService = appService;
            _installService = installService;
        }

        public async Task InstallApp(string userId, int appId, bool hasSubscription)
        {
            // Fetch user and app
            var user = _context.MyUsers.Include(u => u.Installs).FirstOrDefault(u => u.Id == userId);
            if (user == null) throw new Exception("User not found.");

            var app = _context.Apps.FirstOrDefault(a => a.Id == appId);
            if (app == null) throw new Exception("App not found.");

            if (user.Installs.Any(i => i.AppId == appId))
                throw new Exception("App is already installed.");

            double finalPrice = 0;
            if (app.Price != 0)
            {
                finalPrice = hasSubscription ? _appService.GetPriceWithDiscount(appId) : _appService.GetPriceWithoutDiscount(appId);
                
                if (finalPrice > user.AccountBalance)
                    throw new Exception("Insufficient balance. Transaction failed.");

                user.AccountBalance -= finalPrice;
            }

            // Add the app to the installed list
            var install = new Install
            {
                UserId = userId,
                AppId = appId,
                PaymentId = GeneratePaymentId()
            };
            await _installService.CreateInstall(install);
            user.Installs.Add(install);
 
            // Save changes
            _context.SaveChanges();

            Console.WriteLine($"App '{app.Name}' installed successfully. Remaining balance: {user.AccountBalance}");
        }

        private int GeneratePaymentId()
        {
            // placeholder
            return new Random().Next(1000, 9999);
        }

        public void UninstallApp(string userId, int appId)
        {
            var user = _context.MyUsers.Include(u => u.Installs).FirstOrDefault(u => u.Id == userId);
            if (user == null) throw new Exception("User not found.");

            if (!user.Installs.Any())
            {
                Console.WriteLine("You have no installed apps.");
                return;
            }

            var appToRemove = user.Installs.FirstOrDefault(i => i.AppId == appId);
            if (appToRemove == null)
            {
                Console.WriteLine("App not found in your installed apps.");
                return;
            }

            user.Installs.Remove(appToRemove);
            _context.SaveChanges();

            Console.WriteLine("The app has been successfully uninstalled.");
        }
    }
}
