using proiect_licenta.Server.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using proiect_licenta.Models;

namespace proiect_licenta.Models
{
    public class PaymentRecord
    {
        [Key] public int Id { get; set; }
        [Required] public string UserId { get; set; }
        public int AppId { get; set; }
        public DateTime PaymentDT { get; set; }
        public PaymentType PaymentType { get; set; }
        [JsonIgnore] public virtual App App { get; set; }
        [JsonIgnore] public virtual MyUser User { get; set; }
        [JsonIgnore] public virtual Install Install { get; set; }
        [JsonIgnore] public virtual RefundRequest RefundRequest { get; set; }
    }
}
