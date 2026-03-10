using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace AISEP.BLL.Services.Blockchain
{
    /// <summary>
    /// Nethereum output model for the verifyDocument() Smart Contract view function.
    /// Used internally by SepoliaBlockchainService to deserialize the on-chain response.
    /// </summary>
    [FunctionOutput]
    public class VerifyDocumentOutput
    {
        [Parameter("uint256", 1)]
        public BigInteger EntityId { get; set; }

        [Parameter("uint256", 2)]
        public BigInteger Timestamp { get; set; }
    }
}
