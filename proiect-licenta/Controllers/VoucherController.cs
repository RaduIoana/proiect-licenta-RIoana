using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers
{
    [Route("api/Vouchers")]
    [ApiController]
    [Authorize]
    public class VoucherController : ControllerBase
    {
        private readonly VoucherService _voucherService;

        public VoucherController(VoucherService voucherService, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _voucherService = voucherService;
        }

        // GET: api/MyModel
        [HttpGet("get_all_vouchers")]
        public async Task<ActionResult<IEnumerable<Voucher>>> GetAllVouchers()
        {
            // Simulate async database access
            return Ok(await _voucherService.GetAllVouchers());
        }

        // GET: api/MyModel/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Voucher>> GetById(int id)
        {
            return Ok(await _voucherService.GetVoucher(id));
        }
    /*
        [HttpPut]
        public async Task<ActionResult<Voucher>> PutVoucher(Voucher voucher)
        {
            return Ok(await _voucherService.EditTask(voucher));
        }

        [HttpPost]
        public async Task<ActionResult<Voucher>> PostVoucher(Voucher voucher)
        {
            return Ok(await _voucherService.CreateTask(voucher));
        }
    */
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVoucher(int id)
        {
            await _voucherService.DeleteVoucher(id);
            return NoContent();
        }
    }
}
