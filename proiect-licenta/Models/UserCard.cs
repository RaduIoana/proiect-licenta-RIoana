using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models
{
    public class UserCard
    {
        [Required] public string UserId { get; set; }
        [Required] public int CardId { get; set; }
        [JsonIgnore] public virtual MyUser User { get; set; }
        [JsonIgnore] public virtual Card Card { get; set; }
    }
}
