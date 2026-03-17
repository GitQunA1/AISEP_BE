using AISEP.BLL.Settings;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Security.Cryptography;

namespace AISEP.BLL.Services.Blockchain
{
    public class SepoliaBlockchainService : IBlockchainService
    {
        private readonly BlockchainSettings _settings;
        private readonly string _contractAbi;

        public SepoliaBlockchainService(
            IOptions<BlockchainSettings> blockchainSettings,
            IWebHostEnvironment env)
        {
            _settings = blockchainSettings.Value;

            var abiPath = Path.Combine(env.ContentRootPath, "ContractABI.json");
            _contractAbi = File.ReadAllText(abiPath);
        }

        public async Task<string> ComputeFileHashAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            if (stream.CanSeek)
                stream.Position = 0;
            using var sha256 = SHA256.Create();

            var hashBytes = await sha256.ComputeHashAsync(stream);
            return "0x" + BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        public async Task<string> ComputeFileHashFromUrlAsync(string fileUrl)
        {
            using var httpClient = new HttpClient();
            using var stream = await httpClient.GetStreamAsync(fileUrl);
            using var sha256 = SHA256.Create();

            var hashBytes = await sha256.ComputeHashAsync(stream);
            return "0x" + BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        public async Task<string> StoreHashAsync(string fileHash, int entityId)
        {
            var account = new Account(_settings.AdminPrivateKey, 11155111);
            var web3 = new Web3(account, _settings.RpcUrl);

            var contract = web3.Eth.GetContract(_contractAbi, _settings.ContractAddress);
            var storeFunction = contract.GetFunction("storeDocument");

            var estimatedGas = await storeFunction.EstimateGasAsync(
                account.Address,
                null,
                null,
                fileHash,
                new BigInteger(entityId)
            );

            var receipt = await storeFunction.SendTransactionAndWaitForReceiptAsync(
                account.Address,
                estimatedGas,
                new HexBigInteger(0),
                null,
                fileHash,
                new BigInteger(entityId)
            );

            if (receipt.Status.Value == 0)
                throw new Exception("Blockchain transaction failed (reverted).");

            return receipt.TransactionHash;
        }

        public async Task<(int EntityId, long Timestamp)> VerifyDocumentAsync(string fileHash)
        {
            var web3 = new Web3(_settings.RpcUrl);
            var contract = web3.Eth.GetContract(_contractAbi, _settings.ContractAddress);

            var verifyFunction = contract.GetFunction("verifyDocument");
            var result = await verifyFunction.CallDeserializingToObjectAsync<VerifyDocumentOutput>(fileHash);

            return ((int)result.EntityId, (long)result.Timestamp);
        }
    }
}
