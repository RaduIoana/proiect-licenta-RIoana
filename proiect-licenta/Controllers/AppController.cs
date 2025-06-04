using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers
{
    [Route("api/Apps")]
    [ApiController]
    public class AppController(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly AppService _appService;
        
        // dummies for frontend dev
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        
        [HttpGet("test")]
        public async Task<ActionResult<IEnumerable<App>>> GetTestApps()
        {
            return Enumerable.Range(1, 5).Select(index => new App
                {
                    Name = "App" + index,
                    Description = Summaries[index],
                    Price = index,
                    LaunchDate = DateTime.Now.AddDays(index),
                    Rating = index,
                    Discount = 50
                })
                .ToArray();
        }
        
        [HttpGet("get_all_appss")]
        public async Task<ActionResult<IEnumerable<App>>> GetAllApps()
        {
            return Ok(await _appService.GetAllApps());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<App>> GetById(int id)
        {
            return Ok(await _appService.GetApp(id));
        }

        [HttpGet("appInLibraryCheck")]
        public async Task<Boolean> AppInLibraryCheck(int id)
        {
            return await _appService.AppOwned(id);
        }

        [HttpPut]
        public async Task<ActionResult<App>> PutApp(App app)
        {
            return Ok(await _appService.EditApp(app));
        }

        [HttpPost]
        public async Task<ActionResult<App>> PostApp(App app)
        {
            return Ok(await _appService.CreateApp(app));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteApp(int id)
        {
            await _appService.DeleteApp(id);
            return NoContent();
        }
    }
}