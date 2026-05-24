using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace proiect_licenta.Models;

public class License
{
    [Key] public int Id { get; set; }
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public int AppId { get; set; }
    
    public string WalletAddress { get; set; }
    
    public int PaymentId { get; set; }
    
    public DateTime IssuedAt { get; set; }
    
    public string? Tx { get; set; }
    
    public long? TokenId {get; set;}
    
    public string? IpfsUri { get; set; }
    
    public bool Revoked { get; set; }
    
    [JsonIgnore] public virtual App App { get; set; }
    [JsonIgnore] public virtual ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();
}