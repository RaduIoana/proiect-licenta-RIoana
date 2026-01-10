using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[ApiController]
[Route("/api/exe")]
public class ExeController : ControllerBase
{
    private readonly ExeService _service;

    public ExeController(ExeService service)
    {
        _service = service;
    }

    // takes data and app id, and uploads a file to IPFS and associates it w the app
    [Authorize(Roles = "ADMIN, DEVELOPER")]
    [HttpPost("{appId}")]
    public async Task<ActionResult<string>> UploadFile(IFormFile file, int appId)
    {
        return Ok(await _service.UploadFile(file, appId));
    }

    [HttpGet("{cid}")]
    public async Task<IActionResult> GetFile(string cid)
    {
        var (stream, fileName) = await _service.DownloadFile(cid);
        if (stream.CanSeek)
            stream.Position = 0;
        return File(stream, "application/octet-stream", fileName);
    }
}