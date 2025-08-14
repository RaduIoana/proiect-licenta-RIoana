using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nethereum.Signer;
using proiect_licenta.DTOs;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers;

[Authorize]
[Route("api/MetaAuth")]
[ApiController]
public class MetaAuthController : ControllerBase
{
    private readonly LicenseGenerationService _licenseService;
    
    private readonly MetaAuthService _metaAuthService;
    
    public MetaAuthController(LicenseGenerationService licenseService, MetaAuthService metaAuthService)
    {
        _licenseService = licenseService;
        _metaAuthService = metaAuthService;
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

    [HttpGet("getWallet")]
    public async Task<ActionResult<string>> GetWalletAddress()
    {
        return Ok(await _metaAuthService.GetWalletAddress());
    }

    // for testing on local hardhat node only; no need to call from frontend
    [HttpPut("setWallet")]
    public async Task<ActionResult> SetWallet(string walletAddress)
    {
        return Ok(await _metaAuthService.SetWalletAddress(walletAddress));
    }
    
    [HttpPost("verify")]
    [Consumes("application/json")]
    public async Task<IActionResult> Verify([FromBody] SignedMetaMessageDto request)
    {
        string wallet = request.Wallet;
        string signature = request.Signature;
        string message = request.Message;

        if (string.IsNullOrEmpty(wallet) || string.IsNullOrEmpty(signature))
        {
            return BadRequest("Invalid request.");
        }

        // Recover Ethereum address from the signed message
        var signer = new EthereumMessageSigner();
        string recoveredAddress = signer.EncodeUTF8AndEcRecover(message, signature);

        if (string.Equals(wallet, recoveredAddress, StringComparison.OrdinalIgnoreCase))
        {
            // Check if account has a wallet, if not bind it if it's not already used
            if(await _metaAuthService.CheckUserWalletAssociation(wallet))
                return Ok(new { success = true, message = "Authentication successful." });
        }
        
        return Unauthorized(new { success = false, message = "Authentication failed." });
    }

    // some kind of validation endpoint
}