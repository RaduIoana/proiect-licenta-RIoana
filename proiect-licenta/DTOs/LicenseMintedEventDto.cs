using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace proiect_licenta.DTOs;

[Event("LicenseMinted")]
public class LicenseMintedEventDto : IEventDTO
{
    [Parameter("uint256", "licenseId", 1, true)]
    public BigInteger licenseId { get; set; }
}