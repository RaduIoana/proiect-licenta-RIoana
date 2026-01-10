using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using System.Security.Claims;
using Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;
using proiect_licenta.Exceptions;

namespace proiect_licenta.Services;

public class AppService
{
    // add  access checking
    private readonly ApplicationDbContext _context;
    //private readonly PrivilegeChecker _privilegeChecker;
    private readonly Web3 _web3;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, Web3 web3)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _web3 = web3;
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
                                        && pay.LicenseId != null);
        if (paymentRecord == null || paymentRecord.status != "success") { return false; }
        
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

    public async Task<App> CreateApp(App app)
    {
        var ownerAddress = Environment.GetEnvironmentVariable("OWNER__ACCOUNT__ADDR");
        //var ownerAddress = Environment.GetEnvironmentVariable("OWNER__ACCOUNT__ADDR__LOCAL");
        var contractData = JObject.Parse(File.ReadAllText("/app/hardhatproj/artifacts/contracts/AppStore.sol/AppStore.json"));

        var abi = contractData["abi"].ToString();
        //var contractAddress  = Environment.GetEnvironmentVariable("APPSTORE__ADDR__LOCAL");
        var contractAddress  = Environment.GetEnvironmentVariable("APPSTORE__ADDR");

        var func = _web3.Eth.GetContractQueryHandler<ExistsFunction>();
        bool exists = await func.QueryAsync<bool>(contractAddress, new ExistsFunction{Id = app.Id});

        _context.Apps.Add(app);
        await _context.SaveChangesAsync();

        if (app.Price != 0 && !exists)
        {
            // call contract with admin account and add app to mapping
            var contract = _web3.Eth.GetContract(abi, contractAddress);
            var addAppFunction = contract.GetFunction("addApp");

            var balance = await _web3.Eth.GetBalance.SendRequestAsync(_web3.TransactionManager.Account.Address);
            
            Console.WriteLine("Balance in ETH: " + Web3.Convert.FromWei(balance));
            Console.WriteLine("id: " + app.Id + "  price: " + app.Price + "  address:" + ownerAddress);

            var receipt = await addAppFunction.SendTransactionAndWaitForReceiptAsync(
                from: _web3.TransactionManager.Account.Address,
                gas: new HexBigInteger(3000000),
                value: new HexBigInteger(0),
                functionInput: [app.Id, Web3.Convert.ToWei(app.Price), ownerAddress]
            );
            
            Console.WriteLine("Transaction hash:"+ receipt.TransactionHash);
        }
        
        return app;
    }

    public async Task<App> EditApp(App app)
    {
        var existingApp = await _context.Apps.FindAsync(app.Id);
        if (existingApp == null)
            throw new Exception("App does not exist");
            
        var ownerAddress = Environment.GetEnvironmentVariable("OWNER__ACCOUNT__ADDR");
        //var ownerAddress = Environment.GetEnvironmentVariable("OWNER__ACCOUNT__ADDR__LOCAL");
        
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
                functionInput: [app.Id, Web3.Convert.ToWei(app.Price), ownerAddress]
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