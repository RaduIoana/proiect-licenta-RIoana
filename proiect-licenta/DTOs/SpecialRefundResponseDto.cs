namespace proiect_licenta.DTOs;

public class SpecialRefundResponseDto
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public string PaymentType { get; set; }
    public string Username { get; set; }
    public string WalletAddress { get; set; }
    public DateTime RequestDT { get; set; }
    public int AppId { get; set; }
    public string AppName { get; set; }
    public decimal Sum { get; set; }
    public string Status { get; set; }
}