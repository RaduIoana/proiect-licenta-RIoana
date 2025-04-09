using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models
{
    public class Category
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        [JsonIgnore] public virtual ICollection<AppCategory> AppCategories { get; set; }

    }
}
