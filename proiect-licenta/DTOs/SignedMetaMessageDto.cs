namespace proiect_licenta.DTOs;

public class SignedMetaMessageDto
{
    public string Wallet { get; set; } = String.Empty;
    public string Signature { get; set; } = String.Empty;
    public string Message { get; set; } = String.Empty;
}