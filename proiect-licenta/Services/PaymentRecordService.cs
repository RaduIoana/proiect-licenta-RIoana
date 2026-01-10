using System.Security.Claims;
using Ipfs.Http;
using Microsoft.EntityFrameworkCore;
using Nethereum.Util;
using Nethereum.Web3;
using proiect_licenta.Contexts;
using proiect_licenta.DTOs;
using proiect_licenta.Models;
using proiect_licenta.Server.Enums;

namespace proiect_licenta.Services;

public class PaymentRecordService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Web3 _web3;
    private readonly LicenseService _licenseService;
    private readonly IpfsClient _ipfsClient;

    public PaymentRecordService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor,
        Web3 web3, LicenseService licenseService, IpfsClient ipfsClient)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _web3 = web3;
        _licenseService = licenseService;
        _ipfsClient = ipfsClient;
    }

    public async Task<IEnumerable<PaymentRecord>> GetUserPaymentRecords()
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User Not Found");
        var payRecords = await _context.PaymentRecords
            .Where(pr=> pr.UserId == userId).ToListAsync();
        return payRecords;
    }

    public async Task<PaymentRecord> GetPaymentRecord(int id)
    {
        var payRecord = await _context.PaymentRecords.FindAsync(id);
        if (payRecord == null) { throw new Exception(); }
        return payRecord;
    }

    public async Task<PostPaymentRecordResponseDTO> CreatePaymentRecord(PostPaymentRecordRequestDTO request)
    {
        Console.WriteLine("create payment record");
        PaymentRecord payRecord = new PaymentRecord
        {
            UserId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier),
            AppId = request.AppId,
            PaymentDT = DateTime.Now,
            PaymentType = request.PaymentType
        };

        if (payRecord.PaymentType != PaymentType.Free)
            payRecord.Tx = request.Tx;
        
        _context.PaymentRecords.Add(payRecord);
        await _context.SaveChangesAsync();
            
        PostPaymentRecordResponseDTO response = new PostPaymentRecordResponseDTO
        {
            Id = payRecord.Id,
            AppId = payRecord.AppId,
            PaymentType = payRecord.PaymentType
        };

        if (payRecord.PaymentType != PaymentType.Free)
            response.Tx = payRecord.Tx;
        
        return response;
    }

    public async Task<PaymentRecord> EditPaymentRecord(PaymentRecord payRecord)
    {
        var existingPaymentRecord = await _context.PaymentRecords.FindAsync(payRecord.Id);
        if (existingPaymentRecord == null)
            throw new Exception("PaymentRecord does not exist");

        _context.Entry(payRecord).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return payRecord;
    }

    public async Task DeletePaymentRecord(int id)
    {
        var task = await _context.PaymentRecords.FindAsync(id);
        if (task == null)
            throw new Exception("PaymentRecord does not exist");

        _context.PaymentRecords.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task<ConfirmPaymentResponseDto> ConfirmPaymentRecord(int id)
    {
        await _ipfsClient.VersionAsync();
        
        var existingPaymentRecord = await _context.PaymentRecords.FindAsync(id);
        if (existingPaymentRecord == null)
            throw new Exception("Payment record does not exist");
            
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            throw new Exception("User not found");
        var user = await _context.Users.FindAsync(userId);
        
        if (userId != existingPaymentRecord.UserId)
            throw new Exception("Current user not matching record. Verification failed.");

        var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(existingPaymentRecord.Tx);
        if (receipt == null || receipt.Status.Value == 0
                            || !string.Equals(receipt.From, user.WalletAddress, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("receipt failed");
            Console.WriteLine($"receipt: {receipt.Status.Value} - {receipt.From} vs user: {user.WalletAddress}");
            existingPaymentRecord.status = "failed";
            await _context.SaveChangesAsync();
            return new ConfirmPaymentResponseDto
            {
                Success = false
            };
        }
        Console.WriteLine("Transaction hash:"+ receipt.TransactionHash);

        existingPaymentRecord.status = "success";
        
        var tx = await _web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(existingPaymentRecord.Tx);
        existingPaymentRecord.PaymentAmount = UnitConversion.Convert.FromWei(tx.Value.Value);
        
        await _context.SaveChangesAsync();
            
        //create license
        var license = await _licenseService.CreateLicenseAsync(existingPaymentRecord);
        return new ConfirmPaymentResponseDto
        {
            IpfsUri = license.IpfsUri,
            LicenseId = license.Id,
            Success = true
        };
    }
}