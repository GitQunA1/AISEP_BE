using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace AISEP.BLL.Services.Blockchain
{
    [FunctionOutput]
    public class VerifyDocumentOutput
    {
        [Parameter("uint256", 1)]
        public BigInteger StartupId { get; set; }

        [Parameter("uint256", 2)]
        public BigInteger Timestamp { get; set; }

        [Parameter("address[]", 3)]
        public List<string> Owners { get; set; } = new();
    }
}
