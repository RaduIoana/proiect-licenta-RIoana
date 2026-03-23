using System.Security.Claims;
using proiect_licenta.Contexts;
using proiect_licenta.Exceptions;

namespace proiect_licenta.Services;

public class MetaAuthService
{
    // add  access checking
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    //private readonly PrivilegeChecker _privilegeChecker;

    public MetaAuthService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<string> GetUserWalletAddress()
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new NotFoundException("User Not Found");
        return user.WalletAddress;
    }

    // for local node testing, pls don't call this with real addresses
    public async Task<string> SetWalletAddress(string walletAddress)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User Not Found");
        
        user.WalletAddress = walletAddress;
        await _context.SaveChangesAsync();
        
        return "Wallet address updated.";
    }

    public async Task<Boolean> CheckUserWalletAssociation(string walletAddress)
    {
        Console.WriteLine($"backend wallet arrival: {walletAddress}");
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        var user = await _context.MyUsers.FindAsync(userId);
        if(user == null)
            throw new Exception("User not found.");
        
        // check if it's been registered to another user (including this one)
        var reuse = _context.MyUsers.FirstOrDefault(u => u.WalletAddress.Equals(walletAddress));
        Console.WriteLine($"reuse: {reuse}");
        if (reuse != null)
            throw new Exception("Wallet address is already in use.");
        
        user.WalletAddress = walletAddress;
        return await _context.SaveChangesAsync() > 0;
    }
}