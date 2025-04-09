using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using proiect_licenta.Models;

namespace proiect_licenta.Models
{
    public class AppCategory
    {
        [Key][Required] public int CategoryId { get; set; }
        [Required] public int AppId { get; set; }
        [JsonIgnore] public virtual App App { get; set; }
        [JsonIgnore] public virtual Category Category {  get; set; }
    }
}
