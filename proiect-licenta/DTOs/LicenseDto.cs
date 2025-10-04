using Newtonsoft.Json;

namespace proiect_licenta.DTOs;

public class LicenseDto
{
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("description")]
    public string Description { get; set; }
    
    [JsonProperty("appid")]
    public int AppId { get; set; }
    
    [JsonProperty("walletaddress")]
    public string WalletAddress { get; set; }
    
    [JsonProperty("issuedat")]
    public DateTime IssuedAt { get; set; }
    
    [JsonProperty("valid")]
    public bool Valid { get; set; }
    
    [JsonProperty("attributes")]
    public List<NftAttributesDto> Attributes { get; set; } = [];
}

public class NftAttributesDto
{
    [JsonProperty("traitType")]
    public string TraitType { get; set; }
    
    [JsonProperty("value")]
    public string Value { get; set; }
}