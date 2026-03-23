using System.Numerics;
using System.Security.Claims;
using System.Text.Json;
using Ipfs.Http;
using Microsoft.EntityFrameworkCore;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;
using proiect_licenta.Contexts;
using proiect_licenta.DTOs;
using proiect_licenta.Exceptions;
using proiect_licenta.Models;
using proiect_licenta.Server.Enums;

namespace proiect_licenta.Services;

public class LicenseService
{
    private readonly ApplicationDbContext _context;
    private readonly Web3 _web3;
    private readonly IpfsClient _ipfsClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LibraryService _libraryService;

    public LicenseService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IpfsClient ipfsClient, Web3 web3, LibraryService libraryService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _web3 = web3;
        _ipfsClient = ipfsClient;
        _libraryService = libraryService;
    }

    [Function("hasLicense", "bool")]
    private class HasLicenseFunction : FunctionMessage
    {
        [Parameter("address", "user", 1)]
        public string User { get; set; }

        [Parameter("uint256", "appId", 2)]
        public BigInteger AppId { get; set; }
    }
    
    [Function("getLicense", "uint256")]
    private class GetLicenseFunction : FunctionMessage
    {
        [Parameter("address", "user", 1)]
        public string User { get; set; }

        [Parameter("uint256", "appId", 2)]
        public BigInteger AppId { get; set; }
    }
    
    public async Task<decimal> GetBalanceAsync(string walletAddress)
    {
        if (string.IsNullOrWhiteSpace(walletAddress))
            throw new ArgumentException("Invalid wallet address");

        var balanceWei = await _web3.Eth.GetBalance.SendRequestAsync(walletAddress);
        return Web3.Convert.FromWei(balanceWei.Value);
    }

    public async Task<License> CreateLicenseAsync(PaymentRecord record)
    {
        await _ipfsClient.VersionAsync();
        
        var app = await _context.Apps.FindAsync(record.AppId);
        var user = await _context.Users.FindAsync(record.UserId);
        var licenseMetadata = new LicenseDto
        {
            Name = "License for app:" + app.Name,
            Description = "License for app:" + record.AppId,
            AppId = record.AppId,
            WalletAddress = user.WalletAddress,
            IssuedAt = DateTime.UtcNow,
            Valid = true,
            Attributes =
            [
                new() { TraitType = "AppId", Value = app.Id.ToString() },
                new() { TraitType = "UserId", Value = user.Id },
                new() { TraitType = "LicenseType", Value = "Lifetime" }
            ]
        };
        var cid = await UploadLicenseAsync(licenseMetadata);
        
        // license must have token id set later
        var license = new License
        {
            Name = licenseMetadata.Name,
            Description = licenseMetadata.Description,
            AppId = app.Id,
            PaymentId = record.Id,
            IssuedAt = DateTime.Now,
            IpfsUri = cid,
            WalletAddress = user.WalletAddress
        };
        Console.WriteLine("license:" + license.Id + ", " + license.PaymentId);
        _context.Licenses.Add(license);
        await _context.SaveChangesAsync();
        
        Console.WriteLine("Saved license id: " + license.Id);
        Console.WriteLine("License count: " + await _context.Licenses.CountAsync());
        
        return license;
    }

    public async Task<string> UploadLicenseAsync(LicenseDto license)
    {
        var json = JsonSerializer.Serialize(license);
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        var response = await _ipfsClient.FileSystem.AddAsync(stream, "license.json");
        Console.WriteLine("Cid:" + response.Id.Hash);
        return response.Id.Hash.ToString();
    }

    public async Task<LicenseDto> GetLicenseAsync(string cid)
    {
        var stream = await _ipfsClient.FileSystem.ReadFileAsync(cid);
        var json = await new StreamReader(stream).ReadToEndAsync();
        var license = JsonSerializer.Deserialize<LicenseDto>(json);
        Console.WriteLine("License:" + license);
        return license;
    }

    public async Task<MintLicenseResponseDto> MintLicenseAsync(int licenseId, int paymentId)
    {
        await _ipfsClient.VersionAsync();
        
        var contractData =
            JObject.Parse(File.ReadAllText("/app/hardhatproj/artifacts/contracts/LicenseService.sol/LicenseService.json"));

        var abi = contractData["abi"].ToString();
        //var contractAddress  = Environment.GetEnvironmentVariable("LICENSE__ADDR__LOCAL");
        var contractAddress  = Environment.GetEnvironmentVariable("LICENSE__ADDR");

        var license = await _context.Licenses.FindAsync(licenseId);
        if (license == null)
            return new MintLicenseResponseDto
            {
                Success = false,
                Error = "License not found."
            };
        
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return new MintLicenseResponseDto
            {
                Success = false,
                Error = "User not found."
            };
        
        var getFunc = _web3.Eth.GetContractQueryHandler<GetLicenseFunction>();
        // if license is not 0, means it's not valid. so we reactivate it.
        var getLicense = await getFunc.QueryAsync<int>(contractAddress, new GetLicenseFunction
        {
            User = user.WalletAddress,
            AppId = license.AppId
        });

        Console.WriteLine("getLicense:" + getLicense);
        
        if (getLicense != 0)
        {
            await RevalidateLicenseAsync(userId, license, paymentId);
            
            return new MintLicenseResponseDto
            {
                Success = true,
                TxHash = license.Tx
            };
        }

        var hasFunc = _web3.Eth.GetContractQueryHandler<HasLicenseFunction>();
        // check if user already has this license, so it won't be minted again
        var hasLicense = await hasFunc.QueryAsync<bool>(contractAddress, new HasLicenseFunction
        {
            User = user.WalletAddress,
            AppId = license.AppId
        });

        Console.WriteLine("HasLicense:" + hasLicense);
        // this will fail if license validity is 0. (duhhh)
        // this can still work in case another payment is made by mistake somehow.
        // but for my "license exists, but is not valid" check, i only need to check if the licenses array value isn't 0.
        if (hasLicense)
        {
            return new MintLicenseResponseDto
            {
                Success = false,
                Error = "License already exists."
            };
        }

        var contract = _web3.Eth.GetContract(abi, contractAddress);
        var mintLicenseFunction = contract.GetFunction("mintLicense");
        var receipt = await mintLicenseFunction.SendTransactionAndWaitForReceiptAsync(
            from: _web3.TransactionManager.Account.Address,
            gas: new HexBigInteger(600000),
            value: null,
            functionInput: [ user.WalletAddress, license.AppId, license.IpfsUri]
        );
        Console.WriteLine("mintLicense:" + receipt.TransactionHash);
        
        // grabbing return value from events
        var licenseEvent = receipt.DecodeAllEvents<LicenseMintedEventDto>().FirstOrDefault();
        if (licenseEvent == null)
            return new MintLicenseResponseDto
            {
                Success = false,
                Error = "No license minted."
            };
        license.TokenId = (long)licenseEvent.Event.licenseId;
        license.Tx = receipt.TransactionHash;
        Console.WriteLine("token id:" + license.TokenId);

        var paymentRecord = await _context.PaymentRecords.FindAsync(license.PaymentId);
        paymentRecord.LicenseId = license.Id;
        await _context.SaveChangesAsync();
        
        return new MintLicenseResponseDto
        {
            Success = true,
            TxHash = license.Tx
        };
    }

    public async Task<MintLicenseResponseDto> MintFreeLicenseAsync(int appId)
    {
        await _ipfsClient.VersionAsync();
        
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return new MintLicenseResponseDto
            {
                Success = false,
                Error = "User not found."
            };
        
        var app = await _context.Apps.FindAsync(appId);
        if (app == null)
            return new MintLicenseResponseDto
            {
                Success = false,
                Error = "App not found."
            };
        if (app.Price != 0)
            return new MintLicenseResponseDto
            {
                Success = false,
                Error = "App isn't free."
            };
        
        var paymentRecord = await _context.PaymentRecords
            .Where(p => p.AppId == appId 
             && p.UserId == userId 
             && p.PaymentType == PaymentType.Free
             && p.Status != "refunded")
            .FirstOrDefaultAsync();
        if (paymentRecord == null)
            return new MintLicenseResponseDto
            {
                Success = false,
                Error = "Error adding app to library."
            };
        
        var existingLicense = await _context.Licenses.FindAsync(paymentRecord.LicenseId);
        if (existingLicense != null)
        {
            paymentRecord.LicenseId = existingLicense.Id;
            await _context.SaveChangesAsync();
            
            return new MintLicenseResponseDto
            {
                Success = true,
                TxHash = existingLicense.Tx
            };
        }
        
        var license = await CreateLicenseAsync(paymentRecord);
        
        Console.WriteLine("\n");
        Console.WriteLine("minting free license");
        Console.WriteLine("\n");

        paymentRecord.LicenseId = license.Id;
        paymentRecord.Status = "success";
        await _context.SaveChangesAsync();
        
        return new MintLicenseResponseDto
        {
            Success = true,
            TxHash = license.Tx
        };
    }
    
    public async Task RevokeLicenseAsync(string userId, PaymentRecord paymentRecord)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new NotFoundException("User Not Found");
        
        var license = await _context.Licenses.FindAsync(paymentRecord.LicenseId);
        if (license == null)
            throw new NotFoundException("License not found.");
        
        var contractData =
            JObject.Parse(File.ReadAllText("/app/hardhatproj/artifacts/contracts/LicenseService.sol/LicenseService.json"));

        var abi = contractData["abi"].ToString();
        //var contractAddress  = Environment.GetEnvironmentVariable("LICENSE__ADDR__LOCAL");
        var contractAddress  = Environment.GetEnvironmentVariable("LICENSE__ADDR");
        
        var contract = _web3.Eth.GetContract(abi, contractAddress);
        var revokeLicenseFunction = contract.GetFunction("revokeLicense");
        var receipt = await revokeLicenseFunction.SendTransactionAndWaitForReceiptAsync(
            from: _web3.TransactionManager.Account.Address,
            gas: new HexBigInteger(600000),
            value: null,
            functionInput: [user.WalletAddress, license.AppId]
        );
        Console.WriteLine("revokeLicense:" + receipt.TransactionHash);
        
        license.Revoked = true;
        _context.Entry(license).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
    
    private async Task RevalidateLicenseAsync(string userId, License license, int paymentId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new NotFoundException("User Not Found");
        
        var contractData =
            JObject.Parse(File.ReadAllText("/app/hardhatproj/artifacts/contracts/LicenseService.sol/LicenseService.json"));

        var abi = contractData["abi"].ToString();
        //var contractAddress  = Environment.GetEnvironmentVariable("LICENSE__ADDR__LOCAL");
        var contractAddress  = Environment.GetEnvironmentVariable("LICENSE__ADDR");
        
        var contract = _web3.Eth.GetContract(abi, contractAddress);
        var revalidateLicenseFunction = contract.GetFunction("revalidateLicense");
        var receipt = await revalidateLicenseFunction.SendTransactionAndWaitForReceiptAsync(
            from: _web3.TransactionManager.Account.Address,
            gas: new HexBigInteger(600000),
            value: null,
            functionInput: [user.WalletAddress, license.AppId]
        );
        Console.WriteLine("revalidateLicense:" + receipt.TransactionHash);

        _context.Licenses.Attach(license);
        license.Revoked = false;
        
        var paymentRecord = await _context.PaymentRecords.FindAsync(paymentId);
        Console.WriteLine(paymentRecord.Id);
        paymentRecord.LicenseId = license.Id;
        license.PaymentId = paymentRecord.Id;
        
        await _context.SaveChangesAsync();
        
        Console.WriteLine("revalidateLicense:" + receipt.TransactionHash);
    }

    public async Task RevokeFreeLicenseAsync(int appId)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new Exception("User Not Found");
        
        var existingPaymentRecord = await _context.PaymentRecords
            .Include(p => p.License)
            .Where(p => p.AppId == appId 
                        && p.UserId == userId 
                        && p.PaymentType == PaymentType.Free
                        && p.Status != "refunded")
            .FirstOrDefaultAsync();
        if (existingPaymentRecord.PaymentAmount != 0 || existingPaymentRecord.License.Revoked)
            throw new Exception("Cannot remove from library.");
        
        var license = await _context.Licenses.FindAsync(existingPaymentRecord.LicenseId);
        if (license == null)
            throw new NotFoundException("License not found.");
        
        existingPaymentRecord.Status = "refunded";
        license.Revoked = true;
        _context.Entry(license).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}