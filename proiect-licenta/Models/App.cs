using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models
{
    public class App
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DevId { get; set; }
        public string? AppFileCid { get; set; }
        public decimal Price { get; set; }
        public DateTime LaunchDate { get; set; }
        public int Discount { get; set; }

        [JsonIgnore] public virtual AppFile? AppFile { get; set; }
        [JsonIgnore] public virtual ICollection<AppImage> AppImages { get; set; } = new List<AppImage>();
        [JsonIgnore] public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        [JsonIgnore] public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        [JsonIgnore] public virtual ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();
        [JsonIgnore] public virtual ICollection<AppCategory> AppCategories { get; set; } = new List<AppCategory>();
        [JsonIgnore] public virtual ICollection<LibraryRecord> Libraries { get; set; } = new List<LibraryRecord>();
        [JsonIgnore] public virtual ICollection<License> Licenses { get; set; } = new List<License>();
        
    }
}
