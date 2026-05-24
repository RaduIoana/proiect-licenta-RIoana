using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Contexts;
using proiect_licenta.Exceptions;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[Route("api/Apps")]
[ApiController]
public class AppController : ControllerBase
{
    private readonly AppService _appService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _context;

    public AppController(AppService appService, IHttpContextAccessor httpContextAccessor,
        ApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _appService = appService;
        _context = context;
    }
        
    [AllowAnonymous]
    [HttpGet("get_apps")]
    public async Task<ActionResult<IEnumerable<App>>> GetApps([FromQuery] int[]? categories, 
        [FromQuery] string? sortBy, [FromQuery] string? order)
    {
        return Ok(await _appService.GetApps(categories, sortBy, order));
    }

    [HttpGet("get_user_apps")]
    public async Task<ActionResult<IEnumerable<App>>> GetUserApps([FromQuery] int[]? categories, 
        [FromQuery] string? sortBy, [FromQuery] string? order)
    {
        return Ok(await _appService.GetUserApps(categories, sortBy, order));
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<App>> GetById(int id)
    {
        return Ok(await _appService.GetApp(id));
    }

    [AllowAnonymous]
    [HttpGet("rating/{id}")]
    public async Task<ActionResult<float>> GetRating(int id)
    {
        return Ok(await _appService.GetAppRating(id));
    }

    [HttpGet("appInLibraryCheck/{id}")]
    public async Task<Boolean> AppInLibraryCheck(int id)
    {
        return await _appService.AppOwned(id);
    }

    [HttpGet("isAppDeveloper/{id}")]
    public async Task<Boolean> IsAppDeveloper(int id)
    {
        return await _appService.IsAppDeveloper(id);
    }

    [Authorize(Roles = "ADMIN, DEVELOPER")]
    [HttpPut]
    public async Task<ActionResult<App>> PutApp(App app)
    {
        return Ok(await _appService.EditApp(app));
    }

    [Authorize(Roles = "ADMIN, DEVELOPER")]
    [HttpPost]
    public async Task<ActionResult<App>> PostApp(App app)
    {
        // moved user check here so I can use this for db seed
        var userId = _httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found");
        return Ok(await _appService.CreateApp(app, user.Id, user.WalletAddress));
    }

    [Authorize(Roles = "ADMIN, DEVELOPER")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteApp(int id)
    {
        await _appService.DeleteApp(id);
        return NoContent();
    }
}