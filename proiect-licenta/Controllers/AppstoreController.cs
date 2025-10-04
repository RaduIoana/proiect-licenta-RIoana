using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[ApiController]
[Route("api/Appstore")]
public class AppstoreController : ControllerBase
{
    private readonly AppstoreService _appstoreService;
    
    public AppstoreController(AppstoreService appstoreService)
    {
        _appstoreService = appstoreService;
    }

    [HttpGet("install/{appId}")]
    public async Task<ActionResult> InstallApp(int appId)
    {
        var (stream, fileName) = await _appstoreService.InstallApp(appId);
        if (stream.CanSeek)
            stream.Position = 0;
        
        return File(stream, "application/octet-stream", fileName);
    }

    [HttpDelete("remove/{appId}")]
    public async Task<ActionResult> RemoveFromLibrary(int appId)
    {
        await _appstoreService.RemoveFromLibrary(appId);
        return NoContent();
    }

    [HttpGet("price/{appId}")]
    public async Task<ActionResult<double>> GetAppPrice(int appId)
    {
        return Ok(await _appstoreService.CalculateAppPrice(appId));
    }
}