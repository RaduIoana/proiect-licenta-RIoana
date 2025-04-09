using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using proiect_licenta.Models;

namespace proiect_licenta.Models
{
    public class Review
    {
        [Required] public string UserId { get; set; }
        [Required] public int AppId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public float Rating { get; set; }
        public DateTime PostDate { get; set; }
        [JsonIgnore] public virtual App App { get; set; }
        [JsonIgnore] public virtual MyUser User { get; set; }
    }
}
