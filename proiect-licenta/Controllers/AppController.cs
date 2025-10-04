using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[Route("api/Apps")]
[ApiController]
public class AppController : ControllerBase
{
    private readonly AppService _appService;

    public AppController(AppService appService)
    {
        _appService = appService;
    }
        
    // dummies for frontend dev
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
        
    [AllowAnonymous]
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
        
    [AllowAnonymous]
    [HttpGet("get_all_apps")]
    public async Task<ActionResult<IEnumerable<App>>> GetAllApps()
    {
        return Ok(await _appService.GetAllApps());
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<App>> GetById(int id)
    {
        return Ok(await _appService.GetApp(id));
    }

    [Authorize]
    [HttpGet("appInLibraryCheck/{id}")]
    public async Task<Boolean> AppInLibraryCheck(int id)
    {
        return await _appService.AppOwned(id);
    }

    [Authorize]
    [HttpPut]
    public async Task<ActionResult<App>> PutApp(App app)
    {
        return Ok(await _appService.EditApp(app));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<App>> PostApp(App app)
    {
        return Ok(await _appService.CreateApp(app));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteApp(int id)
    {
        await _appService.DeleteApp(id);
        return NoContent();
    }
}