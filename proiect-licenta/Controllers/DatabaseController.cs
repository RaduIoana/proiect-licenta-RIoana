using Ipfs.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Exceptions;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[Route("api/Db/")]
[ApiController]
public class DatabaseController: ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AppService _appService;
    private readonly LicenseService _licenseService;
    private readonly IpfsClient _ipfsClient;
    private readonly UserManager<MyUser> _userManager;

    public DatabaseController(ApplicationDbContext context, AppService appService, LicenseService licenseService,
        IpfsClient ipfsClient, UserManager<MyUser> userManager)
    {
        _context = context;
        _appService = appService;
        _licenseService = licenseService;
        _ipfsClient = ipfsClient;
        _userManager = userManager;
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("reset")]
    public async Task<ActionResult> ResetDatabase()
    {
        // revoke licenses on the chain
        var licenses = await _context.Licenses.ToListAsync();
        foreach (var license in licenses)
        {
            var paymentRecord = await _context.PaymentRecords.FirstOrDefaultAsync(p => p.LicenseId == license.Id);
            if (paymentRecord != null)
                await _licenseService.RevokeLicenseAsync(paymentRecord.UserId, paymentRecord);
        }
        
        // run cleanup script
        await _context.Database.ExecuteSqlRawAsync(System.IO.File.ReadAllText("Sql/cleanup.sql"));
        
        // delete images stored in separate volume
        var imagePaths = await _context.AppImages.Select(a => a.Path).ToListAsync();
        foreach (var path in imagePaths)
        {
            if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path.TrimStart('/'))))
            {
                System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path.TrimStart('/')));
            }
            var image = await _context.AppImages.FirstOrDefaultAsync(a => a.Path == path);
            _context.AppImages.Remove(image);
        }

        // apps require interaction with contract to delete
        var apps = await _appService.GetApps([], null, null);
        foreach (var app in apps)
        {
            // unpin app executable from ipfs
            if (app.AppFileCid != null)
            {
                try
                {
                    await _ipfsClient.Pin.RemoveAsync(app.AppFileCid);
                    await _ipfsClient.Block.RemoveAsync(app.AppFileCid);
                }
                catch (HttpRequestException e)
                {
                    // ignore, it's fine if the file doesn't exist
                }
                var file = await _context.AppFiles.FirstOrDefaultAsync(a => a.Cid == app.AppFileCid);
                _context.AppFiles.Remove(file);
            }
            await _appService.DeleteApp(app.Id);
        }
        await _context.Database.ExecuteSqlRawAsync(
            "ALTER TABLE Apps AUTO_INCREMENT = 1;"
        );
        await _context.Database.ExecuteSqlRawAsync(
            "ALTER TABLE AppImages AUTO_INCREMENT = 1;"
        );
        await _context.SaveChangesAsync();
        
        // seed users and roles
        var admin = new MyUser
        {
            Id = "seed-admin-0001",
            UserName = "admin",
            Email = "admin@gmail.com",
            NormalizedUserName = "ADMIN",
            NormalizedEmail = "ADMIN@GMAIL.COM",
            WalletAddress = "0xba7661dc6603a6d2c1b4ff58d2f5675211c1a7d6"
        };
        await _userManager.CreateAsync(admin, "AdminPassword!123");
        await _userManager.AddToRoleAsync(admin, "ADMIN");
        await _userManager.AddToRoleAsync(admin, "USER");
        
        var developer = new MyUser
        {
            Id = "seed-dev-0001",
            UserName = "developer",
            Email = "developer@gmail.com",
            NormalizedUserName = "DEVELOPER",
            NormalizedEmail = "DEVELOPER@GMAIL.COM",
            WalletAddress = "0x1acF9fa886aaB0D742fff4d9fcA35B208469a254"
        };
        await _userManager.CreateAsync(developer, "DevPassword!123");
        await _userManager.AddToRoleAsync(developer, "DEVELOPER");
        await _userManager.AddToRoleAsync(developer, "USER");
        
        var user1 = new MyUser
        {
            Id = "seed-user-0001",
            UserName = "user1",
            Email = "user1@gmail.com",
            NormalizedUserName = "USER1",
            NormalizedEmail = "USER1@GMAIL.COM",
            WalletAddress = "0xb22B099ECD42fb5aE062F48137CAeFb214534029"
        };
        await _userManager.CreateAsync(user1, "User1Password!123");
        await _userManager.AddToRoleAsync(user1, "USER");
        
        var user2 = new MyUser
        {
            Id = "seed-user-0002",
            UserName = "user2",
            Email = "user2@gmail.com",
            NormalizedUserName = "USER2",
            NormalizedEmail = "USER2@GMAIL.COM",
            WalletAddress = ""
        };
        await _userManager.CreateAsync(user2, "User2Password!123");
        await _userManager.AddToRoleAsync(user2, "USER");
        
        // reseed apps
        var appIds = new List<int>
        {
            (await _appService.CreateApp(new App
            {
                Name = "Angry Birds",
                Description = "The survival of the Angry Birds is at stake. Dish out revenge on the greedy pigs who stole their eggs. Use the unique powers of each bird to destroy the pigs’ defenses. Angry Birds features challenging physics-based gameplay and hours of replay value. Each level requires logic, skill and force to solve.\n\nFEATURES:\n– Enjoy fun and satisfying slingshot gameplay.\n– Play all 15 original Angry Birds episodes – over 680 levels!\n– Compete against other players in the Mighty League.\n– Boost your birds’ destructive strength with powerups.\n– Download and play for free!\n– Play offline!",
                DevId = admin.Id,
                Price = 0,
                LaunchDate = DateTime.Now,
                Discount = 0
            }, admin.Id, admin.WalletAddress)).Id,
            (await _appService.CreateApp(new App
            {
                Name = "Weather",
                Description = "Best way to plan your day\n\nGet the latest weather conditions, whether you're hitting the slopes, the beach or simply checking the forecast for your commute. See accurate hourly, 5-day, and 10-day forecasts for wherever you're going or whatever you’re doing.\n\nDETAILED CONDITIONS\nQuickly access the day drill down, hourly, daily, and 10-day forecasts, and historical weather averages. Check wind, visibility, humidity, barometer, dew point, and chance of precipitation. See sunrise, sunset, moon phase, and UV index.\n\nINTERACTIVE MAPS\nGo deep in detail with temperature, radar observation, radar forecast, precipitation, cloud and satellite maps.\n\nMULTIPLE CITIES\nTrack current weather in all the locations you care about most. Add multiple cities to your favorites for quick access. Sign in to save your preferences.\n\nSEVERE WEATHER ALERTS\nStay informed with timely notifications to help you prepare for severe weather.\n\nAVAILABLE ANYWHERE\nAutomatically sync your favorite cities across MSN Weather on the web and your mobile apps for quick access to the places you care about.",
                DevId = admin.Id,
                Price = new decimal(0.023),
                LaunchDate = DateTime.Now,
                Discount = 0
            }, admin.Id, admin.WalletAddress)).Id,
            (await _appService.CreateApp(new App
            {
                Name = "Calendar",
                Description = "A new event begins the moment you tap a date.\n\nIt helps you create events and tasks quickly and easily, and ensures that you remember them.\n\nDecorate your home screen beautifully with the neat looking transparent widget.\n\n\n[Key Features]\n\n*Manage all your schedules at a glance by adding various Calendars, including Google Calendar.\n\n*Assign color codes to events in each calendar.\n\n*Provides various options to display including year, month, week, day and task views.\n\n*Display weekly weather information.\n\n*Set a pattern of recurrence and the time zone when you create an event.\n\n*Choose from several types of widgets with adjustable transparency.\n\n*Switch from one day, week, month or year to the next with a simple horizontal swipe.\n\n*Set up variety notifications for an event.",
                DevId = developer.Id,
                Price = new decimal(0.0034),
                LaunchDate = DateTime.Now,
                Discount = 0
            }, developer.Id, developer.WalletAddress)).Id,
            (await _appService.CreateApp(new App
            {
                Name = "LinkedIn",
                Description = "Welcome to LinkedIn – The Professional Network That Works for You\n\nBuild your career, grow your network, and unlock opportunities with LinkedIn, your trusted platform for meaningful professional connections.\n\nExplore Key Features:\n\nSeamless Job Search\n- Effortlessly find roles that align with your skills and interests using LinkedIn’s AI-powered tools.\n- Set personalized job alerts to stay ahead of potential opportunities.\n- Quick Apply: Use your LinkedIn profile or resume to apply for jobs in just a few clicks.\n\nEngage With Business News & Insights\n- Stay updated on industry trends and company news to make informed career decisions.\n- Access content shared by industry leaders, explore collaborative articles, and join engaging conversations within your community.\n\nProfessional Networking That Matters\n- Build meaningful connections with colleagues, peers, and industry experts.\n- Engage with a community of professionals who share your interests and goals.\n- Enhanced Visibility: Members who share posts and updates twice a week enjoy up to 5x more profile views, boosting their professional reach.\n\nStrengthen Your Professional Brand\n- Showcase your skills, achievements, and projects to underscore your expertise.\n- Verification Badges: Add a verification badge to signal authenticity and build trust—verified members see 60% more profile views on average.\nJoin a Trusted and Secure Community\n- Grow and engage in a secure platform backed by LinkedIn’s verification features. Verified profiles, workplace credentials, and educational backgrounds create a trusted environment for connections and opportunities.\n- Collaborate with like-minded professionals in a space designed to foster meaningful and productive relationships.",
                DevId = developer.Id,
                Price = 0,
                LaunchDate = DateTime.Now,
                Discount = 0
            }, developer.Id, developer.WalletAddress)).Id
        };
        await _context.SaveChangesAsync();
        
        foreach (var id in appIds)
        {
            Console.WriteLine(id);
            await SeedImages(id);
            await SeedFile(id);
        }
        await _context.SaveChangesAsync();
        
        // run db seed script
        await _context.Database.ExecuteSqlRawAsync(System.IO.File.ReadAllText("Sql/seed.sql"));

        return Ok();
    }

    public async Task<string> SeedImages(int appId)
    {
        var seedSource = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", appId.ToString());
        if (!Directory.Exists(seedSource))
            return "No images to seed";
        
        var destination = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "apps", appId.ToString());
        if (!Directory.Exists(destination))
            Directory.CreateDirectory(destination);

        foreach (var file in Directory.GetFiles(seedSource))
        {
            var extension = Path.GetExtension(Path.GetFileName(file));
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExtensions.Contains(extension))
                continue;
            
            var fileName = Guid.NewGuid() + Path.GetExtension(file);
            System.IO.File.Copy(file, Path.Combine(destination, fileName));
            string urlPath = $"/apps/{appId}/{fileName}";

            if (Path.GetFileName(file).Contains("icon"))
            {
                var appImage = new AppImage
                {
                    ImageType = "icon",
                    AppId = appId,
                    Path = urlPath
                };
                _context.AppImages.Add(appImage);
                await _context.SaveChangesAsync();
            }
            else
            {
                var appImage = new AppImage
                {
                    ImageType = "screenshot",
                    AppId = appId,
                    Path = urlPath
                };
                _context.AppImages.Add(appImage);
                await _context.SaveChangesAsync();
            }
        }
        
        return "Images Seeded";
    }

    public async Task<string> SeedFile(int appId)
    {
        var seedSource = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", appId.ToString());
        if (!Directory.Exists(seedSource))
            return "No images to seed";

        var app = await _context.Apps.FirstOrDefaultAsync(a => a.Id == appId);
        if (app == null)
            throw new NotFoundException("App not found");

        foreach (var file in Directory.GetFiles(seedSource))
        {
            var extension = Path.GetExtension(Path.GetFileName(file));
            if (extension != ".exe") continue;
            
            var stream = System.IO.File.OpenRead(Path.Combine(seedSource, Path.GetFileName(file)));
            var node = await _ipfsClient.FileSystem.AddAsync(stream, Path.GetFileName(file));
            var appFile = new AppFile
            {
                Cid = node.Id.Hash.ToString(),
                FileName = Path.GetFileName(file),
                AppId = appId
            };
                
            _context.AppFiles.Add(appFile);
            app.AppFileCid = appFile.Cid;
            await _context.SaveChangesAsync();
                
            return "File uploaded.";
        }

        return "Couldn't find file to upload.";
    }
}