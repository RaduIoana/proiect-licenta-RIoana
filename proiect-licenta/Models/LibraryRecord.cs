using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models
{
    public class LibraryRecord
    {
        [Required] public string UserId { get; set; }
        [Required] public int AppId { get; set; }
        [Required] public int PaymentId { get; set; }
        [JsonIgnore] public virtual App App { get; set; }
        [JsonIgnore] public virtual MyUser User { get; set; }
        [JsonIgnore] public virtual PaymentRecord PaymentRecord { get; set; }
    }
}
