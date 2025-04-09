using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models
{
    public class UserVoucher
    {
        [Required] public string UserId { get; set; }
        [Required] public int VoucherId { get; set; }
        [JsonIgnore] public virtual MyUser User { get; set; }
        [JsonIgnore] public virtual Voucher Voucher { get; set; }
    }
}
