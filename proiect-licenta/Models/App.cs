using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models
{
    public class App
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //[Required] public int DevId { get; set; }
        public int Price { get; set; }
        public DateTime LaunchDate { get; set; }
        public float Rating { get; set; }
        public int Discount { get; set; }
        
        [JsonIgnore] public virtual ICollection<Review> Reviews { get; set; }
        [JsonIgnore] public virtual ICollection<Report> Reports { get; set; }
        [JsonIgnore] public virtual ICollection<PaymentRecord> PaymentRecords { get; set; }
        [JsonIgnore] public virtual ICollection<AppCategory> AppCategories { get; set; }
        [JsonIgnore] public virtual ICollection<Install> Installs { get; set; }
        
    }
}
