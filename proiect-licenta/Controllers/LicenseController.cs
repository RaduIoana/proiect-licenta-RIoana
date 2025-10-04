using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.DTOs;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[ApiController]
[Route("api/License")]
[Authorize]
public class LicenseController : ControllerBase
{
    private readonly LicenseService _service;


    public LicenseController(LicenseService service)
    {
        _service = service;
    }
/*
    [HttpPost]
    public async Task<ActionResult<LicenseDto>> CreateLicense(LicenseCreationDTO licenseDto)
    {
        return Ok(await _service.CreateLicenseAsync(licenseDto));
    }
*/
    [HttpPost("upload")]
    public async Task<ActionResult<string>> UploadLicense(LicenseDto license)
    {
        return Ok(await _service.UploadLicenseAsync(license));
    }
    
    [HttpGet("mint/{id}")]
    public async Task<ActionResult<string>> MintLicense(int id)
    {
        return Ok(await _service.MintLicenseAsync(id));
    }

    [HttpGet("mintFree/{appId}")]
    public async Task<ActionResult<string>> MintLicenseFree(int appId)
    {
        return Ok(await _service.MintFreeLicenseAsync(appId));
    }
    
    [HttpGet]
    public async Task<ActionResult<MintLicenseResponseDto>> GetLicense(string cid)
    {
        return Ok(await _service.GetLicenseAsync(cid));
    }
}