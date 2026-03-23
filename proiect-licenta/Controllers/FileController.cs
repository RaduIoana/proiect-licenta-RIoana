using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[ApiController]
[Route("/api/file")]
public class FileController : ControllerBase
{
    private readonly FileService _service;

    public FileController(FileService service)
    {
        _service = service;
    }

    // takes data and app id, uploads a file to IPFS and associates it w the app
    [Authorize(Roles = "ADMIN, DEVELOPER")]
    [HttpPost("ipfs/{appId}")]
    public async Task<ActionResult<string>> UploadFileToIpfs(IFormFile file, int appId)
    {
        return Ok(await _service.UploadFileToIpfs(file, appId));
    }

    [HttpGet("ipfs/{cid}")]
    public async Task<IActionResult> GetFileFromIpfs(string cid)
    {
        var (stream, fileName) = await _service.DownloadFileFromIpfs(cid);
        if (stream.CanSeek)
            stream.Position = 0;
        return File(stream, "application/octet-stream", fileName);
    }

    [Authorize(Roles = "ADMIN, DEVELOPER")]
    [HttpPost("images/{type}/{appId}")]
    public async Task<IActionResult> UploadImagesToFilesystem(List<IFormFile> files, int appId, string type)
    {
        Console.WriteLine("post image");
        return Ok(await _service.UploadImagesToFilesystem(files, appId, type));
    }

    [AllowAnonymous]
    [HttpGet("images/{type}/{appId}")]
    public async Task<ActionResult<string[]>> GetImagesFromFilesystem(int appId, string type)
    {
        return Ok(await _service.GetImagesFromFilesystem(appId, type));
    }
}