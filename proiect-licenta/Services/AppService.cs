using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using System.Security.Claims;
using Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;

namespace proiect_licenta.Services
{
    public class AppService
    {
        // add  access checking
        private readonly ApplicationDbContext _context;
        //private readonly PrivilegeChecker _privilegeChecker;
        private readonly ClaimsPrincipal _user;
        private readonly Web3 _web3;

        public AppService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, Web3 web3)
        {
            _context = context;
            _user = httpContextAccessor.HttpContext!.User;
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
            if (app == null) { throw new Exception(); }
            return app;
        }

        public async Task<Boolean> AppOwned(int id)
        {
            var userId = _user.FindFirstValue(ClaimTypes.NameIdentifier);
            var inLibrary = _context.Installs.Any(inst => inst.UserId == userId && inst.AppId == id);
            return inLibrary;
        }

        public async Task<App> CreateApp(App app)
        {
            var ownerAddress = Environment.GetEnvironmentVariable("OWNER__ACCOUNT__ADDR");
            var contractData = JObject.Parse(File.ReadAllText("/app/hardhatproj/artifacts/contracts/BuyApp.sol/BuyApp.json"));
                
            var abi = contractData["abi"].ToString();
            var contractAddress  = "0x5FbDB2315678afecb367f032d93F642f64180aa3";  // this is local addr
            
            var func = _web3.Eth.GetContractQueryHandler<ExistsFunction>();
            bool exists = await func.QueryAsync<bool>(contractAddress, new ExistsFunction{Id = app.Id});
            Console.WriteLine(exists);
            
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
                    value: null,
                    functionInput: new object[] {app.Id, Web3.Convert.ToWei(app.Price), ownerAddress}
                );
                // setting address to owner address is placeholder until implementing vendor accs
            
                Console.WriteLine("Transaction hash:"+ receipt.TransactionHash);
            }
            
            return app;
        }

        public async Task<App> EditApp(App app)
        {
            //check privilege
            var existingApp = await _context.Apps.FindAsync(app.Id);
            if (existingApp == null)
                throw new Exception("App does not exist");
            
            var ownerAddress = Environment.GetEnvironmentVariable("OWNER__ACCOUNT__ADDR");
            var contractData = JObject.Parse(File.ReadAllText("/app/hardhatproj/artifacts/contracts/BuyApp.sol/BuyApp.json"));
                
            var abi = contractData["abi"].ToString();
            var contractAddress  = "0x5FbDB2315678afecb367f032d93F642f64180aa3";  // this is local addr
            
            var func = _web3.Eth.GetContractQueryHandler<ExistsFunction>();
            bool exists = await func.QueryAsync<bool>(contractAddress, new ExistsFunction{Id = app.Id});
            Console.WriteLine(exists);

            if (exists)
            {
                var contract = _web3.Eth.GetContract(abi, contractAddress);
                var updateAppFunction = contract.GetFunction("updateApp");
                var receipt = await updateAppFunction.SendTransactionAndWaitForReceiptAsync(
                    from: _web3.TransactionManager.Account.Address,
                    gas: new HexBigInteger(3000000),
                    value: null,
                    functionInput: new object[] {app.Id, Web3.Convert.ToWei(app.Price), ownerAddress}
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
            var contractData = JObject.Parse(File.ReadAllText("/app/hardhatproj/artifacts/contracts/BuyApp.sol/BuyApp.json"));
                
            var abi = contractData["abi"].ToString();
            var contractAddress  = "0x5FbDB2315678afecb367f032d93F642f64180aa3";  // this is local addr
            
            var app = await _context.Apps.FindAsync(id);
            if (app == null)
                throw new Exception("App does not exist");
            
            var func = _web3.Eth.GetContractQueryHandler<ExistsFunction>();
            bool exists = await func.QueryAsync<bool>(contractAddress, new ExistsFunction{Id = app.Id});
            Console.WriteLine(exists);
            
            if (exists)
            {
                var contract = _web3.Eth.GetContract(abi, contractAddress);
                var deleteAppFunction = contract.GetFunction("deleteApp");
                var receipt = await deleteAppFunction.SendTransactionAndWaitForReceiptAsync(
                    from: _web3.TransactionManager.Account.Address,
                    gas: new HexBigInteger(3000000),
                    value: null,
                    functionInput: new object[] {app.Id}
                );
            
                Console.WriteLine("Transaction hash:"+ receipt.TransactionHash);
            }

            _context.Apps.Remove(app);
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
