using System.Security.Claims;
using Nethereum.Web3;
using proiect_licenta.Contexts;

namespace proiect_licenta.Services;

public class LicenseGenerationService
{
    private readonly ApplicationDbContext _context;
    private readonly ClaimsPrincipal _user;
    private readonly Web3 _web3;

    public LicenseGenerationService(ApplicationDbContext context, ClaimsPrincipal user)
    {
        _context = context;
        _user = user;
        _web3 = new Web3("https://mainnet.infura.io/v3/d0830a3002cf4e00a65cb05ce6b31cf8");
    }
    
    public async Task<decimal> GetBalanceAsync(string walletAddress)
    {
        if (string.IsNullOrWhiteSpace(walletAddress))
            throw new ArgumentException("Invalid wallet address");

        var balanceWei = await _web3.Eth.GetBalance.SendRequestAsync(walletAddress);
        return Web3.Convert.FromWei(balanceWei.Value);
    }
    
    // method here for communicating with the hardhat project that generates the nft
    
    // another service for payments?
    
}