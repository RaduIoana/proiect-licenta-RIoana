using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using proiect_licenta.Enums;

namespace proiect_licenta.Models
{
    public class RefundRequest
    {
        [Key] public int Id { get; set; }
        [Required] public int PaymentId { get; set; }
        [Required] public string UserId { get; set; }
        public DateTime RequestDT { get; set; }
        public RefundStatus Status { get; set; }
        public string? Tx { get; set; }
        [JsonIgnore] public virtual MyUser User { get; set; }
        [JsonIgnore] public virtual PaymentRecord PaymentRecord { get; set; }
    }
}
