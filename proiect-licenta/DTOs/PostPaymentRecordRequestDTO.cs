using proiect_licenta.Server.Enums;

namespace proiect_licenta.DTOs;

public class PostPaymentRecordRequestDTO
{
    public int AppId { get; set; }
    public PaymentType PaymentType { get; set; }
    public string? Tx { get; set; }
}