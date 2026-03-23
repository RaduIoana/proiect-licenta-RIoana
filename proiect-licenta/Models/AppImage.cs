using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models;

public class AppImage
{
    [Key] public int Id { get; set; }
    public int AppId { get; set; }
    public string ImageType { get; set; }
    public string Path { get; set; }
    [JsonIgnore] public virtual App? App { get; set; }
}