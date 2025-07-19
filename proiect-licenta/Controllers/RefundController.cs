using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers
{
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

        // GET: api/MyModel
        [HttpGet("get_all_refunds")]
        public async Task<ActionResult<IEnumerable<RefundRequest>>> GetAllRefunds()
        {
            // Simulate async database access
            return Ok(await _refundService.GetAllRefunds());
        }

        // GET: api/MyModel/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RefundRequest>> GetById(int id)
        {
            return Ok(await _refundService.GetRefund(id));
        }

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
    }

}
