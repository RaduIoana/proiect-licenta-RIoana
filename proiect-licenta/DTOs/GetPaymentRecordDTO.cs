using proiect_licenta.Server.Enums;

namespace proiect_licenta.DTOs;

public class GetPaymentRecordDTO
{
    public int Id { get; set; }
    public int AppId { get; set; }
    public DateTime PaymentDT { get; set; }
    public PaymentType PaymentType { get; set; }
    public decimal? PaymentAmount { get; set; }
    public string PaymentStatus { get; set; }
    public bool CanRefund { get; set; }
}