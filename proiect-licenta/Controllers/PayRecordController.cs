using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers
{
    [Route("api/PaymentRecords")]
    [ApiController]
    [Authorize]
    public class PayRecordController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly PayRecordService _paymentRecordService;

        // GET: api/MyModel
        [HttpGet("get_all_payRecords")]
        public async Task<ActionResult<IEnumerable<PaymentRecord>>> GetAllPaymentRecords()
        {
            // Simulate async database access
            return Ok(await _paymentRecordService.GetAllPaymentRecords());
        }

        // GET: api/MyModel/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentRecord>> GetById(int id)
        {
            return Ok(await _paymentRecordService.GetPaymentRecord(id));
        }

        [HttpPut]
        public async Task<ActionResult<PaymentRecord>> PutPaymentRecord(PaymentRecord payRecord)
        {
            return Ok(await _paymentRecordService.EditPaymentRecord(payRecord));
        }

        [HttpPost]
        public async Task<ActionResult<PaymentRecord>> PostPaymentRecord(PaymentRecord payRecord)
        {
            return Ok(await _paymentRecordService.CreatePaymentRecord(payRecord));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePaymentRecord(int id)
        {
            await _paymentRecordService.DeletePaymentRecord(id);
            return NoContent();
        }
    }

}
