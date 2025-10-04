using System.Security.Claims;
using proiect_licenta.Contexts;
using proiect_licenta.Models;

namespace proiect_licenta.Services;

public class UserAccService
{
    // add  access checking
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    //private readonly PrivilegeChecker _privilegeChecker;
    //private readonly ClaimsPrincipal _user;

    public UserAccService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    // REVAMP - most of it is moving to hardhat probably
    
    /*
    // renew used to switch between query / purchase modes?? why
    // query - just check how much u have left, purchase - actually buy it
    public async Task<string> CheckSubscriptionStatus(bool renew, int monthsToAdd = 0)
    {
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User Not Found");

        if (user.Subscription && !renew)
            return $"Your subscription expires on: {DateTime.Now.AddDays(user.DaysLeft)}";
        if (!renew)
            return "You do not have a subscription.";
            
        return RenewSubscription(user, monthsToAdd);
    }

    private string RenewSubscription(MyUser user, int monthsToAdd)
    {
        double finalAmount = CalculateSubscriptionPrice(monthsToAdd);
            
        if (finalAmount > user.AccountBalance)
            throw new Exception("Insufficient balance for this transaction.");
            
        user.Subscription = true;
        user.AccountBalance -= finalAmount;
        user.DaysLeft += monthsToAdd * 30;
            
        _context.SaveChanges();

        return $"Subscription renewed! New account balance: {user.AccountBalance}. Days remaining: {user.DaysLeft}.";
    }

    private double CalculateSubscriptionPrice(int monthsToAdd)
    {
        if (monthsToAdd <= 0)
            throw new ArgumentException("Months to add must be greater than 0.");
        
        double totalPrice = monthsToAdd * 25;

        // Apply discount (5% per month, capped at 50%)
        var discount = Math.Min(totalPrice / 2, monthsToAdd * 5.0 / 100 * totalPrice);
        return totalPrice - discount;
    }
        
    private void AddMoneyFromVoucher(MyUser user, Voucher voucher)
    {
        if (voucher.Balance <= 0) throw new Exception("Voucher has been used.");

        user.AccountBalance += voucher.Balance;
        voucher.Balance = 0;
            
        _context.Update(voucher);
    }
    */
}