namespace proiect_licenta.DTOs;

public class MintLicenseResponseDto
{
    public bool Success { get; set; }
    public string? TxHash { get; set; }
    public string? Error { get; set; }
}