using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;
using proiect_licenta.DTOs;
using proiect_licenta.Enums;
using proiect_licenta.Exceptions;
using proiect_licenta.Server.Enums;

namespace proiect_licenta.Services;

public class RefundService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<MyUser> _userManager;
    private readonly LicenseService _licenseService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Web3 _web3;
        
    public RefundService(ApplicationDbContext context, UserManager<MyUser> userManager,
        Web3 web3, LicenseService licenseService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _userManager = userManager;
        _web3 = web3;
        _licenseService = licenseService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<RefundRequest>> GetAllRefunds()
    {
        var refunds = await _context.RefundRequests
            .ToListAsync();
        return refunds;
    }

    public async Task<IEnumerable<SpecialRefundResponseDto>> GetAllRefundsAdmin()
    {
        var refunds = await _context.RefundRequests
            .Include(r => r.User)
            .Include(r => r.PaymentRecord)
                .ThenInclude(p => p.App)
            .ToListAsync();
        
        var specialRefunds = new List<SpecialRefundResponseDto>();

        foreach (var refund in refunds)
        {
            var specialRefund = new SpecialRefundResponseDto
            {
                Id = refund.Id,
                PaymentId = refund.PaymentId,
                PaymentType = refund.PaymentRecord.PaymentType.ToString(),
                Username = refund.User.UserName,
                WalletAddress = refund.User.WalletAddress,
                RequestDT = refund.RequestDT,
                AppId = refund.PaymentRecord.AppId,
                AppName = refund.PaymentRecord.App.Name,
                Sum = refund.PaymentRecord.PaymentAmount ?? 0,
                Status = refund.Status.ToString()
            };
            specialRefunds.Add(specialRefund);
        }
        
        return specialRefunds;
    }

    public async Task<IEnumerable<RefundRequest>> GetAllRefundsUser()
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User Not Found");
        var refunds = await _context.RefundRequests.Where(r => r.UserId == userId).ToListAsync();
        return refunds;
    }

    public async Task<RefundRequest> GetRefund(int id)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found");
        
        var refund = await _context.RefundRequests.FindAsync(id);
        if (refund == null) { throw new Exception("Refund not found"); }
        
        if (refund.UserId != userId || _userManager.GetRolesAsync(user).Result.Contains("ADMIN"))
            throw new Exception("Unauthorized");
        
        return refund;
    }

    public async Task<RefundRequest> CreateRefund(RefundRequest refund)
    {
        _context.RefundRequests.Add(refund);
        await _context.SaveChangesAsync();
        return refund;
    }

    public async Task<RefundRequest> EditRefund(RefundRequest refund)
    {
        var existingRefund = await _context.RefundRequests.FindAsync(refund.Id);
        if (existingRefund == null)
            throw new Exception("Refund request does not exist");

        _context.Entry(refund).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return refund;
    }

    public async Task DeleteRefund(int id)
    {
        var refund = await _context.RefundRequests.FindAsync(id);
        if (refund == null)
            throw new Exception("Refund request does not exist");

        _context.RefundRequests.Remove(refund);
        await _context.SaveChangesAsync();
    }

    public async Task<RefundRequest> RequestRefund(int paymentId)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.Include(u => u.Libraries)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new Exception("User Not Found");
            
        var paymentRecord = await _context.PaymentRecords.FindAsync(paymentId);
        if (paymentRecord == null)
            throw new Exception("Payment Not Found");
        if (paymentRecord.UserId != userId)
            throw new Exception("User Not Authorized");
        if (paymentRecord.PaymentAmount is null)
            throw new Exception("Payment cannot be refunded.");
            
        var app = await _context.Apps
            .FirstOrDefaultAsync(a => a.Id == paymentRecord.AppId);
        if (app == null) 
            throw new Exception("App not found.");
            
        // check if refund is issued within 24 hours
        if ((DateTime.Now - paymentRecord.PaymentDT).Hours > 24)
            throw new Exception("Refund period exceeded.");
        
        Console.WriteLine("Refund request created");
            
        return await CreateRefund(new RefundRequest {
            PaymentId = paymentRecord.Id,
            UserId = userId,
            RequestDT = DateTime.Now,
            Status = RefundStatus.Processing
        });
    }
    
    public async Task<RefundRequest> GrantRefund(int refundId)
    {
        var refundRequest = await _context.RefundRequests.FindAsync(refundId);
        if (refundRequest == null)
            throw new NotFoundException("Refund request Not Found");
        
        var user = await _context.Users.Include(u => u.Libraries)
            .FirstOrDefaultAsync(u => u.Id == refundRequest.UserId);
        if (user == null)
            throw new NotFoundException("User Not Found");
            
        var paymentRecord = await _context.PaymentRecords.Include(p => p.User)
            .Include(p => p.License)
            .FirstOrDefaultAsync(p => p.Id == refundRequest.PaymentId);
        if (paymentRecord == null)
            throw new NotFoundException("Payment Not Found");
        
        Console.WriteLine("Id: " + paymentRecord.Id + "Amount: " + paymentRecord.PaymentAmount + "   Revoked? " + paymentRecord.License.Revoked);
        
        if (paymentRecord.PaymentAmount is null || paymentRecord.License.Revoked)
            throw new Exception("Payment cannot be refunded.");
        
        var ownerAddress = Environment.GetEnvironmentVariable("OWNER__ACCOUNT__ADDR");
        //var ownerAddress = Environment.GetEnvironmentVariable("OWNER__ACCOUNT__ADDR__LOCAL");
        var contractData = JObject.Parse(File.ReadAllText("/app/hardhatproj/artifacts/contracts/AppStore.sol/AppStore.json"));
                
        var abi = contractData["abi"].ToString();
        //var contractAddress = Environment.GetEnvironmentVariable("APPSTORE__ADDR__LOCAL");
        var contractAddress  = Environment.GetEnvironmentVariable("APPSTORE__ADDR");
        
        var contract = _web3.Eth.GetContract(abi, contractAddress);
        var refundAppFunction = contract.GetFunction("refundApp");
        
        Console.WriteLine(new HexBigInteger(Web3.Convert.ToWei(paymentRecord.PaymentAmount ?? 0, UnitConversion.EthUnit.Wei)));
        var receipt = await refundAppFunction.SendTransactionAndWaitForReceiptAsync(
            from: _web3.TransactionManager.Account.Address,
            gas: new HexBigInteger(3000000),
            value: new HexBigInteger(Web3.Convert.ToWei(paymentRecord.PaymentAmount ?? 0)),
            functionInput: [paymentRecord.AppId, paymentRecord.User.WalletAddress]
        );
        // setting address to owner address is placeholder until implementing vendor accs
        
        Console.WriteLine("Transaction hash:"+ receipt.TransactionHash);

        var refundFinalization = receipt.DecodeAllEvents<RefundFinalizedEventDto>().FirstOrDefault();
        if (refundFinalization == null)
            throw new Exception("Refund could not be finalized.");
        refundRequest.Tx = receipt.TransactionHash;
        refundRequest.Status = RefundStatus.Complete;
        
        //revoke library record
        var libraryRecord = user.Libraries.FirstOrDefault(l => l.UserId == user.Id && l.PaymentId == refundRequest.PaymentId);
        user.Libraries.Remove(libraryRecord);
        
        // revoke license
        await _licenseService.RevokeLicenseAsync(user.Id, paymentRecord);

        await _context.SaveChangesAsync();
        return refundRequest;
    }
}