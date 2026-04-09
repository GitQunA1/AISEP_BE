using AISEP.BLL.Settings;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Common;
using Microsoft.Extensions.Options;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Hex.HexTypes;
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

        public async Task<string> RegisterDocumentAsync(string fileHash, int startupId)
        {
            var account = new Account(_settings.AdminPrivateKey, 11155111);
            var web3 = new Web3(account, _settings.RpcUrl);

            var contract = web3.Eth.GetContract(_contractAbi, _settings.ContractAddress);
            var registerFunction = contract.GetFunction("registerDocument");

            var estimatedGas = await registerFunction.EstimateGasAsync(
                account.Address,
                null,
                null,
                fileHash,
                new BigInteger(startupId)
            );

            var receipt = await registerFunction.SendTransactionAndWaitForReceiptAsync(
                account.Address,
                estimatedGas,
                new HexBigInteger(0),
                null,
                fileHash,
                new BigInteger(startupId)
            );

            if (receipt.Status.Value == 0)
                throw new InvalidOperationException("Blockchain transaction failed (reverted).");

            return receipt.TransactionHash;
        }

        public async Task<string> AssignDocumentOwnerAsync(string fileHash, string investorWallet)
        {
            if (string.IsNullOrWhiteSpace(fileHash))
            {
                throw new InvalidOperationException("Document hash is required for blockchain owner assignment.");
            }

            if (string.IsNullOrWhiteSpace(investorWallet))
            {
                throw new InvalidOperationException("Investor wallet is required for blockchain owner assignment.");
            }

            try
            {
                var account = new Account(_settings.AdminPrivateKey, 11155111);
                var web3 = new Web3(account, _settings.RpcUrl);

                var contract = web3.Eth.GetContract(_contractAbi, _settings.ContractAddress);
                var addOwnerFunction = contract.GetFunction("addDocumentOwner");

                var estimatedGas = await addOwnerFunction.EstimateGasAsync(
                    account.Address,
                    null,
                    null,
                    fileHash,
                    investorWallet);

                var receipt = await addOwnerFunction.SendTransactionAndWaitForReceiptAsync(
                    account.Address,
                    estimatedGas,
                    new HexBigInteger(0),
                    null,
                    fileHash,
                    investorWallet);

                if (receipt.Status.Value == 0)
                {
                    throw new InvalidOperationException("Blockchain transaction failed (reverted).");
                }

                return receipt.TransactionHash;
            }
            catch (SmartContractRevertException ex)
            {
                throw new InvalidOperationException($"addDocumentOwner reverted: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Cannot connect to blockchain RPC endpoint: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new InvalidOperationException($"Blockchain request timed out: {ex.Message}", ex);
            }
        }

        public async Task<(int StartupId, long Timestamp, IReadOnlyList<string> Owners)> VerifyDocumentAsync(string fileHash)
        {
            var web3 = new Web3(_settings.RpcUrl);
            var contract = web3.Eth.GetContract(_contractAbi, _settings.ContractAddress);

            var verifyFunction = contract.GetFunction("verifyDocument");
            try
            {
                var result = await verifyFunction.CallDeserializingToObjectAsync<VerifyDocumentOutput>(fileHash);
                return ((int)result.StartupId, (long)result.Timestamp, result.Owners);
            }
            catch (SmartContractRevertException ex)
            {
                if (ex.Message.Contains("Document hash not found", StringComparison.OrdinalIgnoreCase))
                {
                    // Hash chua ton tai tren chain -> tra ve "not found" an toan.
                    return (0, 0, Array.Empty<string>());
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
                    var (startupId, timestamp, _) = await VerifyDocumentAsync(doc.FileHash!);
                    var isVerified = timestamp > 0 && startupId == projectId;

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
    }
}
