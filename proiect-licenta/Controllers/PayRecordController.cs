using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Contexts;
using proiect_licenta.DTOs;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[Route("api/PaymentRecords")]
[ApiController]
[Authorize]
public class PayRecordController : ControllerBase
{
    private readonly PaymentRecordService _paymentRecordService;

    public PayRecordController(PaymentRecordService paymentRecordService)
    {
        _paymentRecordService = paymentRecordService;
    }
    
    [HttpGet("get_payRecords/user")]
    public async Task<ActionResult<IEnumerable<PaymentRecord>>> GetUserPaymentRecords()
    {
        return Ok(await _paymentRecordService.GetUserPaymentRecords());
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentRecord>> GetById(int id)
    {
        return Ok(await _paymentRecordService.GetPaymentRecord(id));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut]
    public async Task<ActionResult<PostPaymentRecordResponseDTO>> PutPaymentRecord(PaymentRecord payRecord)
    {
        return Ok(await _paymentRecordService.EditPaymentRecord(payRecord));
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentRecordResponseDTO>> PostPaymentRecord(PostPaymentRecordRequestDTO payRecord)
    {
        return Ok(await _paymentRecordService.CreatePaymentRecord(payRecord));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePaymentRecord(int id)
    {
        await _paymentRecordService.DeletePaymentRecord(id);
        return NoContent();
    }

    [HttpPost("confirm/{id}")]
    public async Task<ActionResult<ConfirmPaymentResponseDto>> ConfirmPaymentRecord(int id)
    {
        return Ok(await _paymentRecordService.ConfirmPaymentRecord(id));
    }
}