using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using System.Security.Claims;
using Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;
using proiect_licenta.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace proiect_licenta.Services;

public class AppService
{
    private readonly ApplicationDbContext _context;
    private readonly Web3 _web3;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<MyUser> _userManager;

    public AppService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor,
        Web3 web3, UserManager<MyUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _web3 = web3;
        _userManager = userManager;
    }

    public async Task<IEnumerable<App>> GetApps(int[]? categories, string? sortBy, string? order,
        bool library = false, bool devApps = false)
    {
        var query = _context.Apps.AsQueryable();

        if (library)
        {
            var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.Libraries)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new NotFoundException("User Not Found");
            
            query = _context.Libraries.Where(l => l.UserId == userId)
                .Select(l => l.App).AsQueryable();
        }

        if (devApps)
        {
            var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.Libraries)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new NotFoundException("User Not Found");
            
            query = _context.Apps.Where(a => a.DevId == userId).AsQueryable();
        }

        if (categories?.Length != 0)
        {
            query = query.Where(app =>
                app.AppCategories
                    .Where(ac => categories.Contains(ac.CategoryId))
                    .Select(ac => ac.CategoryId).Distinct()
                    .Count() == categories.Length
            );
        }
        
        query = sortBy switch
        {
            "Name" => order == "Descending" ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name),
            "Price" => order == "Descending" ? query.OrderByDescending(a => a.Price) : query.OrderBy(a => a.Price),
            "Free" => query.Where(a => a.Price == 0),
            "Launch-date" => order == "Descending" ? query.OrderByDescending(a => a.LaunchDate) : query.OrderBy(a => a.LaunchDate),
            "Discount" => order == "Descending" ? query.OrderByDescending(a => a.Discount) : query.OrderBy(a => a.Discount),
            _ => query
        };

        return await query.ToListAsync();
    }

    public async Task<App> GetApp(int id)
    {
        var app = await _context.Apps.FindAsync(id);
        if (app == null) { throw new NotFoundException("App not found"); }
        return app;
    }

    public async Task<float> GetAppRating(int id)
    {
        var app = await _context.Apps.FindAsync(id);
        if (app == null) { throw new NotFoundException("App not found"); }

        var reviews = _context.Reviews.Where(r => r.AppId == id);
        if (!reviews.Any()) return 0;
        
        return await reviews.AverageAsync(r => r.Rating);
    }

    public async Task<Boolean> AppOwned(int id)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        if (user == null) { return false; }
        
        var app = _context.Apps.FindAsync(id).Result;
        if (app == null) { return false; }
        
        var paymentRecord = await _context.PaymentRecords
            .FirstOrDefaultAsync(pay => pay.AppId == id 
                                        && pay.UserId == user.Id
                                        && pay.LicenseId != null
                                        && pay.Status != "refunded");
        if (paymentRecord == null || paymentRecord.Status != "success") { return false; }
        
        var inLibrary = _context.Libraries.Any(inst => inst.UserId == userId && inst.AppId == id);
        if (!inLibrary) { return false; }
        
        // check for license if app is paid
        if (app.Price != 0)
        {
            var license = await _context.Licenses.FirstOrDefaultAsync(l => l.AppId == id && l.PaymentId == paymentRecord.Id);
            if (license == null || license.Revoked) { return false; }
        }

        return true;
    }

    public async Task<Boolean> IsAppDeveloper(int id)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        if (user == null) { return false; }
        
        var app = _context.Apps.FindAsync(id).Result;
        if (app == null) { return false; }

        if (app.DevId == userId || _userManager.GetRolesAsync(user).Result.Contains("ADMIN"))
            return true;
        
        return false;
    }

    public async Task<App> CreateApp(App app, string developerId, string walletAddress)
    {
        var contractData = JObject.Parse(File.ReadAllText("/app/hardhatproj/artifacts/contracts/AppStore.sol/AppStore.json"));

        var abi = contractData["abi"].ToString();
        //var contractAddress  = Environment.GetEnvironmentVariable("APPSTORE__ADDR__LOCAL");
        var contractAddress  = Environment.GetEnvironmentVariable("APPSTORE__ADDR");

        var func = _web3.Eth.GetContractQueryHandler<ExistsFunction>();
        bool exists = await func.QueryAsync<bool>(contractAddress, new ExistsFunction{Id = app.Id});

        app.DevId = developerId;

        _context.Apps.Add(app);
        await _context.SaveChangesAsync();

        if (app.Price != 0 && !exists)
        {
            // call contract with admin account and add app to mapping
            var contract = _web3.Eth.GetContract(abi, contractAddress);
            var addAppFunction = contract.GetFunction("addApp");

            var receipt = await addAppFunction.SendTransactionAndWaitForReceiptAsync(
                from: _web3.TransactionManager.Account.Address,
                gas: new HexBigInteger(3000000),
                value: new HexBigInteger(0),
                functionInput: [app.Id, Web3.Convert.ToWei(app.Price), walletAddress]
            );
            
            Console.WriteLine("Transaction hash:"+ receipt.TransactionHash);
        }
        
        return app;
    }

    public async Task<App> EditApp(App app)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found");
        
        var existingApp = await _context.Apps.FindAsync(app.Id);
        if (existingApp == null)
            throw new Exception("App does not exist");
        
        if (existingApp.DevId != userId && !_userManager.GetRolesAsync(user).Result.Contains("ADMIN"))
            throw new ForbiddenException("Forbidden");
        
        var contractData = JObject.Parse(File.ReadAllText("/app/hardhatproj/artifacts/contracts/AppStore.sol/AppStore.json"));
                
        var abi = contractData["abi"].ToString();
        //var contractAddress  = Environment.GetEnvironmentVariable("APPSTORE__ADDR__LOCAL");
        var contractAddress  = Environment.GetEnvironmentVariable("APPSTORE__ADDR");

        var func = _web3.Eth.GetContractQueryHandler<ExistsFunction>();
        bool exists = await func.QueryAsync<bool>(contractAddress, new ExistsFunction{Id = app.Id});

        if (exists)
        {
            var contract = _web3.Eth.GetContract(abi, contractAddress);
            var updateAppFunction = contract.GetFunction("updateApp");
            var receipt = await updateAppFunction.SendTransactionAndWaitForReceiptAsync(
                from: _web3.TransactionManager.Account.Address,
                gas: new HexBigInteger(3000000),
                value: null,
                functionInput: [app.Id, Web3.Convert.ToWei(app.Price), user.WalletAddress]
            );
            // setting address to owner address is placeholder until implementing vendor accs
            
            Console.WriteLine("Transaction hash:"+ receipt.TransactionHash);
        }

        _context.Entry(existingApp).State = EntityState.Detached;
        _context.Entry(app).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return app;
    }

    public async Task DeleteApp(int id)
    {
        var contractData = JObject.Parse(File.ReadAllText("/app/hardhatproj/artifacts/contracts/AppStore.sol/AppStore.json"));
                
        var abi = contractData["abi"].ToString();
        //var contractAddress = Environment.GetEnvironmentVariable("APPSTORE__ADDR__LOCAL");
        var contractAddress  = Environment.GetEnvironmentVariable("APPSTORE__ADDR");

        var app = await _context.Apps.FindAsync(id);
        if (app == null)
            throw new Exception("App does not exist");
            
        var func = _web3.Eth.GetContractQueryHandler<ExistsFunction>();
        bool exists = await func.QueryAsync<bool>(contractAddress, new ExistsFunction{Id = app.Id});
            
        if (exists)
        {
            var contract = _web3.Eth.GetContract(abi, contractAddress);
            var deleteAppFunction = contract.GetFunction("deleteApp");
            var receipt = await deleteAppFunction.SendTransactionAndWaitForReceiptAsync(
                from: _web3.TransactionManager.Account.Address,
                gas: new HexBigInteger(3000000),
                value: null,
                functionInput: [app.Id]
            );
            
            Console.WriteLine("Transaction hash:"+ receipt.TransactionHash);
        }

        _context.Apps.Remove(app);
        await _context.SaveChangesAsync();
    }

    public decimal GetPriceWithDiscount(int appId)
    {
        var app = _context.Apps.Find(appId);
        if (app == null)
            throw new Exception("App does not exist");

        return app.Price - (app.Price * app.Discount / 100);
    }

    public decimal GetPriceWithoutDiscount(int appId)
    {
        var app = _context.Apps.Find(appId);
        if (app == null)
            throw new Exception("App does not exist");

        return app.Price;
    }
}