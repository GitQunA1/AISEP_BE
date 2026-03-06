using AISEP.Settings;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Security.Cryptography;

namespace AISEP.Services.Blockchain
{
    /// <summary>
    /// Implementation: Kết nối Ethereum Sepolia Testnet, lưu hash lên Smart Contract.
    /// Log chi tiết nằm bên trong service con này.
    /// </summary>
    public class SepoliaBlockchainService : IBlockchainService
    {
        private readonly BlockchainSettings _settings;
        private readonly ILogger<SepoliaBlockchainService> _logger;
        private readonly string _contractAbi;

        public SepoliaBlockchainService(
            IOptions<BlockchainSettings> blockchainSettings,
            ILogger<SepoliaBlockchainService> logger,
            IWebHostEnvironment env)
        {
            _settings = blockchainSettings.Value;
            _logger = logger;

            var abiPath = Path.Combine(env.ContentRootPath, "ContractABI.json");
            _contractAbi = File.ReadAllText(abiPath);
        }

        public async Task<string> ComputeFileHashAsync(IFormFile file)
        {
            _logger.LogInformation("Computing SHA-256 hash for file '{FileName}' ({Size} bytes)...",
                file.FileName, file.Length);

            using var stream = file.OpenReadStream();
            using var sha256 = SHA256.Create();

            var hashBytes = await sha256.ComputeHashAsync(stream);
            var hexString = "0x" + BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            _logger.LogInformation("Hash computed for '{FileName}': {Hash}", file.FileName, hexString);
            return hexString;
        }

        public async Task<string> StoreHashAsync(string fileHash, int entityId)
        {
            _logger.LogInformation("Storing hash on Sepolia for entityId {EntityId}...", entityId);

            // Connect to Sepolia via RPC with the admin wallet
            var account = new Account(_settings.AdminPrivateKey, 11155111); // Sepolia chain ID
            var web3 = new Web3(account, _settings.RpcUrl);

            // Load the smart contract
            var contract = web3.Eth.GetContract(_contractAbi, _settings.ContractAddress);

            // Get the storeDocument function
            var storeFunction = contract.GetFunction("storeDocument");

            // Call storeDocument(string _fileHash, uint256 _startupId) and wait for receipt
            var receipt = await storeFunction.SendTransactionAndWaitForReceiptAsync(
                account.Address,
                null, // gas (auto-estimate)
                null, // value (no ETH sent)
                fileHash,
                new BigInteger(entityId));

            if (receipt.Status.Value == 0)
            {
                _logger.LogError("Blockchain transaction reverted for entityId {EntityId}, hash {Hash}", entityId, fileHash);
                throw new Exception("Blockchain transaction failed (reverted).");
            }

            _logger.LogInformation("Blockchain TX confirmed: {TxHash}", receipt.TransactionHash);
            return receipt.TransactionHash;
        }

        public async Task<(int EntityId, long Timestamp)> VerifyDocumentAsync(string fileHash)
        {
            _logger.LogInformation("Verifying document on blockchain — FileHash: {Hash}", fileHash);

            var web3 = new Web3(_settings.RpcUrl);
            var contract = web3.Eth.GetContract(_contractAbi, _settings.ContractAddress);

            var verifyFunction = contract.GetFunction("verifyDocument");
            var result = await verifyFunction.CallDeserializingToObjectAsync<VerifyDocumentOutput>(fileHash);

            _logger.LogInformation("Blockchain verify result — EntityId: {EntityId}, Timestamp: {Timestamp}",
                result.EntityId, result.Timestamp);

            return ((int)result.EntityId, (long)result.Timestamp);
        }
    }

}
