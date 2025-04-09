using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models
{
    public class Card
    {
        [Key] public int Id { get; set; }
        public string CardNumber { get; set; }
        public DateTime ExpiryDate {  get; set; }
        public string CardName { get; set; }
        [JsonIgnore] public virtual ICollection<UserCard> UserCards { get; set; }
    }
}
