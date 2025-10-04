namespace proiect_licenta.DTOs;

public class ConfirmPaymentResponseDto
{
    public bool Success { get; set; }
    public string? IpfsUri { get; set; }
    public int? LicenseId { get; set; }
}