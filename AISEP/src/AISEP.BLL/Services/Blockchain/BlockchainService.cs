using AISEP.BLL.Settings;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Common;
using Microsoft.Extensions.Options;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Security.Cryptography;

namespace AISEP.BLL.Services.Blockchain
{
    public class BlockchainService : IBlockchainService
    {
        private readonly BlockchainSettings _settings;
        private readonly string _contractAbi;
        private readonly IUnitOfWork _unitOfWork;

        public BlockchainService(
            IOptions<BlockchainSettings> blockchainSettings,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment env)
        {
            _settings = blockchainSettings.Value;
            _unitOfWork = unitOfWork;

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
                throw new InvalidOperationException("Blockchain transaction failed (reverted).");

            return receipt.TransactionHash;
        }

        public async Task<(string TokenId, string TxHash)> MintCertificateAsync(string ownerWallet, string metadataUri)
        {
            var account = new Account(_settings.AdminPrivateKey, 11155111);
            var web3 = new Web3(account, _settings.RpcUrl);

            var contract = web3.Eth.GetContract(_contractAbi, _settings.ContractAddress);
            Nethereum.Contracts.Function mintFunction;
            try
            {
                mintFunction = contract.GetFunction("mintCertificate");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("mintCertificate function was not found in ContractABI.json.", ex);
            }

            var estimatedGas = await mintFunction.EstimateGasAsync(
                account.Address,
                null,
                null,
                ownerWallet,
                metadataUri);

            var receipt = await mintFunction.SendTransactionAndWaitForReceiptAsync(
                account.Address,
                estimatedGas,
                new HexBigInteger(0),
                null,
                ownerWallet,
                metadataUri);

            if (receipt.Status.Value == 0)
                throw new InvalidOperationException("Mint NFT transaction failed (reverted).");

            var tokenId = TryExtractTokenId(receipt)
                ?? throw new InvalidOperationException("Mint NFT succeeded but TokenId was not found in transaction logs.");

            return (tokenId, receipt.TransactionHash);
        }

        public async Task<(int EntityId, long Timestamp)> VerifyDocumentAsync(string fileHash)
        {
            var web3 = new Web3(_settings.RpcUrl);
            var contract = web3.Eth.GetContract(_contractAbi, _settings.ContractAddress);

            var verifyFunction = contract.GetFunction("verifyDocument");
            try
            {
                var result = await verifyFunction.CallDeserializingToObjectAsync<VerifyDocumentOutput>(fileHash);
                return ((int)result.EntityId, (long)result.Timestamp);
            }
            catch (SmartContractRevertException ex)
            {
                if (ex.Message.Contains("Document hash not found", StringComparison.OrdinalIgnoreCase))
                {
                    // Hash chua ton tai tren chain -> tra ve "not found" an toan.
                    return (0, 0);
                }

                throw new InvalidOperationException($"Blockchain verify failed: {ex.Message}");
            }
        }

        public async Task<ProjectBlockchainVerificationResponse> VerifyProjectDocumentsAsync(int projectId)
        {
            var project = await _unitOfWork.Projects.GetByIdWithDocumentsAsync(projectId)
                ?? throw new KeyNotFoundException("Project not found.");

            var allDocuments = project.Documents
                .OrderByDescending(d => d.VerifiedAt)
                .ToList();

            if (!allDocuments.Any())
            {
                return new ProjectBlockchainVerificationResponse
                {
                    IsFullyVerified = false,
                    TotalDocuments = 0,
                    VerifiedDocuments = 0
                };
            }

            var web3 = new Web3(_settings.RpcUrl);
            var verifiedDetails = new List<VerifiedProjectDocumentDto>();
            var unverifiedIds = new List<int>();

            foreach (var doc in allDocuments)
            {
                if (string.IsNullOrWhiteSpace(doc.FileHash) || string.IsNullOrWhiteSpace(doc.BlockchainTxHash))
                {
                    unverifiedIds.Add(doc.DocumentId);
                    continue;
                }

                try
                {
                    var (entityId, timestamp) = await VerifyDocumentAsync(doc.FileHash!);
                    var isVerified = timestamp > 0 && entityId == projectId;

                    if (!isVerified)
                    {
                        unverifiedIds.Add(doc.DocumentId);
                        continue;
                    }

                    var signerAddress = string.Empty;
                    if (!string.IsNullOrWhiteSpace(doc.BlockchainTxHash))
                    {
                        var tx = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(doc.BlockchainTxHash);
                        signerAddress = tx?.From ?? string.Empty;
                    }

                    verifiedDetails.Add(new VerifiedProjectDocumentDto
                    {
                        DocumentId = doc.DocumentId,
                        TxHash = doc.BlockchainTxHash ?? string.Empty,
                        TimestampOnBlockchain = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                        SignerAddress = signerAddress
                    });
                }
                catch (HttpRequestException ex)
                {
                    throw new InvalidOperationException($"Khong the ket noi Blockchain node: {ex.Message}");
                }
                catch (TaskCanceledException ex)
                {
                    throw new InvalidOperationException($"RPC timeout khi xac minh du lieu tren Blockchain: {ex.Message}");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Xac minh Blockchain that bai: {ex.Message}");
                }
            }

            var isFullyVerified = verifiedDetails.Count == allDocuments.Count && unverifiedIds.Count == 0;

            return new ProjectBlockchainVerificationResponse
            {
                IsFullyVerified = isFullyVerified,
                TotalDocuments = allDocuments.Count,
                VerifiedDocuments = verifiedDetails.Count,
                VerifiedDocumentDetails = verifiedDetails,
                UnverifiedDocumentIds = unverifiedIds
            };
        }

        private static string? TryExtractTokenId(TransactionReceipt receipt)
        {
            const string transferEventTopic0 = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef";

            foreach (var logObject in receipt.Logs)
            {
                if (logObject is not FilterLog log || log.Topics is null || log.Topics.Length < 4)
                {
                    continue;
                }

                var topic0 = log.Topics[0]?.ToString();
                if (!string.Equals(topic0, transferEventTopic0, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var tokenTopic = log.Topics[3]?.ToString();
                if (string.IsNullOrWhiteSpace(tokenTopic))
                {
                    continue;
                }

                return new HexBigInteger(tokenTopic).Value.ToString();
            }

            return null;
        }
    }
}
