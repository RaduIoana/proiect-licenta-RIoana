using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Contexts;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers
{
    [Route("api/Auth")]
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
        public async Task<ActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            return Ok(await _authService.Register(registerRequest));
        }
    
        [HttpPost("login")]
        [Consumes("application/json")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            return Ok(await _authService.Login(loginRequest));
        }
    }
    
}