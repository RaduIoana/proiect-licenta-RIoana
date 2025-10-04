using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models
{
    public class MyUser : IdentityUser
    {
        public bool Subscription {get; set;} = false;
        public string WalletAddress { get; set; } = "";
        public int DaysLeft { get; set; } = 0;
        
        [JsonIgnore] public virtual ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
        [JsonIgnore] public virtual ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
        [JsonIgnore] public virtual ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();
        [JsonIgnore] public virtual ICollection<LibraryRecord> Libraries { get; set; } = new List<LibraryRecord>();
        [JsonIgnore] public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        [JsonIgnore] public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
