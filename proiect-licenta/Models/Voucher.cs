using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models
{
    public class Voucher
    {
        [Key] public int Key { get; set; }
        public int Balance { get; set; }
        public DateTime ExpiryDate { get; set; }
        [JsonIgnore] public virtual ICollection<UserVoucher> UserVouchers { get; set; }
    }
}
