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
    
    [Function("mintLicense", "uint256")]
    public class MintLicenseFunction : FunctionMessage
    {
        [Parameter("address", "buyer", 1)]
        public string Buyer { get; set; }

        [Parameter("uint256", "appId", 2)]
        public BigInteger AppId { get; set; }

        [Parameter("string", "tokenURI", 3)]
        public string TokenURI { get; set; }
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
        Console.WriteLine("metadata:" + licenseMetadata.ToString());
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
        Console.WriteLine("license:" + license.ToString());
        _context.Licenses.Add(license);
        await _context.SaveChangesAsync();
        
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

    public async Task<MintLicenseResponseDto> MintLicenseAsync(int licenseId)
    {
        //var ownerAddress = Environment.GetEnvironmentVariable("OWNER__ACCOUNT__ADDR");
        var ownerAddress = Environment.GetEnvironmentVariable("OWNER__ACCOUNT__ADDR__LOCAL");
        var contractData =
            JObject.Parse(File.ReadAllText("/app/hardhatproj/artifacts/contracts/BuyApp.sol/BuyApp.json"));

        var abi = contractData["abi"].ToString();
        var contractAddress = "0x5FbDB2315678afecb367f032d93F642f64180aa3"; // this is local addr

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
        

        var func = _web3.Eth.GetContractQueryHandler<HasLicenseFunction>();
        // check if user already has this license, so it won't be minted again
        bool hasLicense = await func.QueryAsync<bool>(contractAddress, new HasLicenseFunction
        {
            User = user.WalletAddress,
            AppId = license.AppId
        });

        if (hasLicense)
            return new MintLicenseResponseDto
            {
                Success = false,
                Error = "License already owned."
            };

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
        if (app.Price == 0)
            return new MintLicenseResponseDto
            {
                Success = false,
                Error = "App isn't free."
            };
        
        var paymentRecord = await _context.PaymentRecords
            .Where(p => p.AppId == appId 
             && p.UserId == userId 
             && p.PaymentType == PaymentType.Free)
            .FirstOrDefaultAsync();
        if (paymentRecord == null)
            return new MintLicenseResponseDto
            {
                Success = false,
                Error = "Error adding app to library."
            };
        
        var libraryRecord = await _context.Libraries
            .Where(l => l.AppId == appId
             && l.UserId == userId
             && l.PaymentId == paymentRecord.Id)
            .FirstOrDefaultAsync();
        if (libraryRecord == null)
            return new MintLicenseResponseDto
            {
                Success = false,
                Error = "Error adding app to library."
            };
        
        var licenseMetadata = new LicenseDto
        {
            Name = "License for app:" + app.Name,
            Description = "License for app:" + paymentRecord.AppId,
            AppId = paymentRecord.AppId,
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
        
        var license = new License
        {
            Name = "License for app:" + app.Name,
            Description = "License for app:" + paymentRecord.AppId,
            AppId = app.Id,
            PaymentId = paymentRecord.Id,
            IpfsUri = cid,
            IssuedAt = DateTime.Now,
            WalletAddress = user.WalletAddress
        };
        Console.WriteLine("license:" + license);
        _context.Licenses.Add(license);
        
        paymentRecord.LicenseId = license.Id;
        await _context.SaveChangesAsync();
        return new MintLicenseResponseDto
        {
            Success = true,
            TxHash = license.Tx
        };
    }
}