﻿using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models
{
    public class MyUser : IdentityUser
    {
        public bool Subscription {get; set;} = false;
        public string WalletAddress { get; set; } = "";
        public int DaysLeft { get; set; } = 0;
        public double AccountBalance { get; set; } = 0.0;

        [JsonIgnore] public virtual ICollection<UserCard> UserCards { get; set; }
        [JsonIgnore] public virtual ICollection<UserVoucher> UserVouchers { get; set; }
        [JsonIgnore] public virtual ICollection<RefundRequest> RefundRequests { get; set; }
        [JsonIgnore] public virtual ICollection<PaymentRecord> PaymentRecords { get; set; }
        [JsonIgnore] public virtual ICollection<Install> Installs { get; set; }
        [JsonIgnore] public virtual ICollection<Report> Reports { get; set; }
        [JsonIgnore] public virtual ICollection<Review> Reviews { get; set; }
    }
}
