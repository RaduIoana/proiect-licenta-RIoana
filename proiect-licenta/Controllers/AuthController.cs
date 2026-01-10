using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Contexts;
using proiect_licenta.DTOs;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[Route("api/Auth")]
[AllowAnonymous]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    [Consumes("application/json")]
    public async Task<ActionResult> Register([FromBody] RegisterRequestDTO registerRequest)
    {
        return Ok(new { token = await _authService.Register(registerRequest)});
    }
    
    [HttpPost("login")]
    [Consumes("application/json")]
    public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        return Ok(new { token = await _authService.Login(loginRequest)});
    }
}