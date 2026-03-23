using Microsoft.AspNetCore.Mvc;
using proiect_licenta.DTOs;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[Route("api/Library")]
[ApiController]
public class LibraryController : ControllerBase
{
    private readonly LibraryService _service;

    public LibraryController(LibraryService service)
    {
        _service = service;
    }

    [HttpGet("full")]
    public async Task<ActionResult<IEnumerable<LibraryRecord>>> GetAllLibsByUserIdAsync()
    {
        return Ok(await _service.GetAllLibsByUserId());
    }

    [HttpPost]
    public async Task<ActionResult<LibraryDto>> PostLibrary(LibraryDto library)
    {
        return Ok(await _service.CreateLibraryRecord(library));
    }

    [HttpDelete("{appId}")]
    public async Task<ActionResult> DeleteLibrary(int appId)
    {
        await _service.DeleteLibraryRecord(appId);
        return NoContent();
    }
}