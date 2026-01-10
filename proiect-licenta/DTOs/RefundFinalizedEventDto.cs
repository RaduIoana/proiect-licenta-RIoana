using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace proiect_licenta.DTOs;

[Event("RefundFinalized")]
public class RefundFinalizedEventDto : IEventDTO
{
    [Parameter("address", "from", 1, false)]
    public string from { get; set; }
    
    [Parameter("address", "to", 2, false)]
    public string to { get; set; }
    
    [Parameter("uint256", "sum", 3, false)]
    public BigInteger sum { get; set; }

}