using Microsoft.AspNetCore.Mvc;
using Nethereum.Signer;
using Newtonsoft.Json.Linq;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[Route("api/MetaAuth")]
[ApiController]
public class MetaAuthController : ControllerBase
{
    private readonly LicenseGenerationService _licenseService;
    
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MetaAuthController(LicenseGenerationService licenseService, IHttpContextAccessor httpContextAccessor)
    {
        _licenseService = licenseService;
        _httpContextAccessor = httpContextAccessor;
    }
    
    [HttpGet("{walletAddress}")]
    public async Task<IActionResult> GetBalance(string walletAddress)
    {
        try
        {
            var balance = await _licenseService.GetBalanceAsync(walletAddress);
            return Ok(new { wallet = walletAddress, balance });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
    
    [HttpPost("verify")]
    [Consumes("application/json")]
    public IActionResult Verify([FromBody] JObject request)
    {
        string wallet = request["wallet"]?.ToString();
        string signature = request["signature"]?.ToString();
        string message = "Authenticate with my app";

        if (string.IsNullOrEmpty(wallet) || string.IsNullOrEmpty(signature))
        {
            return BadRequest("Invalid request.");
        }

        // Recover Ethereum address from the signed message
        var signer = new EthereumMessageSigner();
        string recoveredAddress = signer.EncodeUTF8AndEcRecover(message, signature);

        if (string.Equals(wallet, recoveredAddress, StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new { success = true, message = "Authentication successful." });
        }
        else
        {
            return Unauthorized(new { success = false, message = "Authentication failed." });
        }
    }
}