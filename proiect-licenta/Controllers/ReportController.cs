using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers
{
    [Route("api/Reports")]
    [ApiController]
    [Authorize]
    public class ReportController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly ReportService _reportService;

        // GET: api/MyModel
        [HttpGet("get_all_reports")]
        public async Task<ActionResult<IEnumerable<Report>>> GetAllReports()
        {
            // Simulate async database access
            return Ok(await _reportService.GetAllReports());
        }

        // GET: api/MyModel/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetById(int id)
        {
            return Ok(await _reportService.GetReport(id));
        }
/*
        [HttpPut]
        public async Task<ActionResult<Report>> PutReport(Report report)
        {
            return Ok(await _reportService.EditReport(report));
        }
*/
        [HttpPost]
        public async Task<ActionResult<Report>> PostReport(Report report)
        {
            return Ok(await _reportService.CreateReport(report));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReport(int id)
        {
            await _reportService.DeleteReport(id);
            return NoContent();
        }
    }

}
