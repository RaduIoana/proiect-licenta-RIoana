using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models;

public class MyUser : IdentityUser
{
    public string WalletAddress { get; set; } = "";

    [JsonIgnore] public virtual ICollection<App> Apps { get; set; } = new List<App>();
    [JsonIgnore] public virtual ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
    [JsonIgnore] public virtual ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();
    [JsonIgnore] public virtual ICollection<LibraryRecord> Libraries { get; set; } = new List<LibraryRecord>();
    [JsonIgnore] public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}