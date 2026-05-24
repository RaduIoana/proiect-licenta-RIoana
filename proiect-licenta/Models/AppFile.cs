using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models;

public class AppFile
{
    [Key] public string Cid { get; set; }
    public string FileName { get; set; }
    public int AppId { get; set; }
    [JsonIgnore] public virtual App App { get; set; }
}