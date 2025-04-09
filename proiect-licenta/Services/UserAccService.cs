using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;

namespace proiect_licenta.Services
{
    public class UserAccService
    {
        // add  access checking
        private readonly ApplicationDbContext _context;
        //private readonly PrivilegeChecker _privilegeChecker;
        //private readonly ClaimsPrincipal _user;

        public UserAccService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
        }
        
        public IEnumerable<string> ShowInstalledApps(string userId)
        {
            var installs = _context.Installs
                .Where(i => i.UserId == userId)
                .Include(i => i.App) // Assuming there is a navigation property to App
                .ToList();

            if (!installs.Any())
                throw new Exception("No installed apps found for this user.");

            var installedApps = installs.Select(i => i.App.Name).ToList(); // Assuming App has a Name property
            return installedApps;
        }

        public string CheckSubscriptionStatus(int userId, bool renew, int monthsToAdd = 0)
        {
            // Fetch the user
            var user = _context.MyUsers.Find(userId);
            if (user == null)
                throw new Exception("User not found.");

            if (user.Subscription)
            {
                var status = $"You have {user.DaysLeft} days left in your subscription.";
                if (!renew)
                    return status;

                // Renew subscription
                return RenewSubscription(user, monthsToAdd);
            }
            else
            {
                var status = "You do not have a subscription.";
                if (!renew)
                    return status;

                // Buy subscription
                return BuySubscription(user, monthsToAdd);
            }
        }

        private string RenewSubscription(MyUser user, int monthsToAdd)
        {
            double finalAmount = CalculateSubscriptionPrice(monthsToAdd);

            // Ensure the user has enough balance to renew
            if (finalAmount > user.AccountBalance)
                throw new Exception("Insufficient balance for this transaction.");

            // Deduct the amount and update subscription details
            user.AccountBalance -= finalAmount;
            user.DaysLeft += monthsToAdd * 30; // Assuming 30 days in a month

            // Save changes to the database
            _context.SaveChanges();

            return $"Subscription renewed! New account balance: {user.AccountBalance}. Days remaining: {user.DaysLeft}.";
        }

        private string BuySubscription(MyUser user, int monthsToAdd)
        {
            double finalAmount = CalculateSubscriptionPrice(monthsToAdd);

            // Ensure the user has enough balance to purchase
            if (finalAmount > user.AccountBalance)
                throw new Exception("Insufficient balance for this transaction.");

            // Deduct the amount and set subscription details
            user.AccountBalance -= finalAmount;
            user.Subscription = true;
            user.DaysLeft = monthsToAdd * 30;

            // Save changes to the database
            _context.SaveChanges();

            return $"Subscription purchased! New account balance: {user.AccountBalance}. Days remaining: {user.DaysLeft}.";
        }

        private double CalculateSubscriptionPrice(int monthsToAdd)
        {
            if (monthsToAdd <= 0)
                throw new ArgumentException("Months to add must be greater than 0.");

            double basePrice = 25;
            double totalPrice = monthsToAdd * basePrice;

            // Apply discount (5% per month, capped at 50%)
            double discount = Math.Min(totalPrice / 2, monthsToAdd * 5 / 100 * totalPrice);
            return totalPrice - discount;
        }

        /*  These should be replaced with a proper module or smth
        public void AddMoneyToAccount(MyUser user, string userPassword, int methodChoice, double amount, string cardPassword = null)
        {
            // Fetch user and their payment methods
            //var user = _context.MyUsers.Include(u => u.UserCards).Include(u => u.UserVouchers).FirstOrDefault(u => u.Id == userId);
            if (user == null) throw new Exception("User not found.");
            if (user.Password != userPassword) throw new Exception("Incorrect user password.");

            // Validate the payment method choice
            if (methodChoice < 0 || methodChoice >= user.Cards.Count + user.Vouchers.Count)
                throw new Exception("Invalid payment method selected.");

            // Process based on whether the choice is a card or voucher
            
            if (methodChoice < user.Cards.Count)
            {
                var card = user.Cards.ElementAt(methodChoice);
                AddMoneyFromCard(user, card, amount, cardPassword);
            }
            else
            {
                var voucherIndex = methodChoice - user.Cards.Count;
                var voucher = user.Vouchers.ElementAt(voucherIndex);
                AddMoneyFromVoucher(user, voucher);
            }

            // Save changes
            _context.SaveChanges();
        }

        private void AddMoneyFromCard(MyUser user, Card card, double amount, string cardPassword)
        {
            if (card.RequiresPassword)
            {
                if (string.IsNullOrEmpty(cardPassword) || cardPassword != card.Password)
                    throw new Exception("Incorrect card password.");
            }

            if (amount > card.Balance) throw new Exception("Insufficient funds on the card.");

            card.Balance -= amount;
            user.AccountBalance += amount;

            // Persist changes
            _context.Update(card);
        }
        */
        
        // this one is ok
        private void AddMoneyFromVoucher(MyUser user, Voucher voucher)
        {
            if (voucher.Balance <= 0) throw new Exception("Voucher has no balance.");

            user.AccountBalance += voucher.Balance;
            voucher.Balance = 0;

            // Persist changes
            _context.Update(voucher);
        }
    }
}
