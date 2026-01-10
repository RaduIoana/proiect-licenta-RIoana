using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.DTOs;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[Route("api/refunds")]
[ApiController]
[Authorize]
public class RefundController : ControllerBase
{
    private readonly RefundService _refundService;

    public RefundController(RefundService refundService)
    {
        _refundService = refundService;
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("get_all_refunds")]
    public async Task<ActionResult<IEnumerable<RefundRequest>>> GetAllRefunds()
    {
        return Ok(await _refundService.GetAllRefunds());
    }
    
    [Authorize(Roles = "ADMIN")]
    [HttpGet("get_all_refunds/admin")]
    public async Task<ActionResult<IEnumerable<SpecialRefundResponseDto>>> GetAllRefundsAdmin()
    {
        return Ok(await _refundService.GetAllRefundsAdmin());
    }
    
    [HttpGet("get_all_refunds/user")]
    public async Task<ActionResult<IEnumerable<RefundRequest>>> GetAllUserRefunds()
    {
        return Ok(await _refundService.GetAllRefundsUser());
    }

    // GET: api/MyModel/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<RefundRequest>> GetById(int id)
    {
        return Ok(await _refundService.GetRefund(id));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut]
    public async Task<ActionResult<RefundRequest>> PutRefund(RefundRequest refund)
    {
        return Ok(await _refundService.EditRefund(refund));
    }

    [HttpPost]
    public async Task<ActionResult<RefundRequest>> PostRefund(RefundRequest refund)
    {
        return Ok(await _refundService.CreateRefund(refund));
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRefund(int id)
    {
        await _refundService.DeleteRefund(id);
        return NoContent();
    }
        
    [HttpPost("refund/{paymentId}")]
    public async Task<ActionResult<RefundRequest>> RequestRefund(int paymentId)
    {
        return Ok(await _refundService.RequestRefund(paymentId));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("grant/{refundId}")]
    public async Task<ActionResult<RefundRequest>> GrantRefund(int  refundId)
    {
        return Ok(await _refundService.GrantRefund(refundId));
    }
}