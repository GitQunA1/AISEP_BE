namespace AISEP.Services.Blockchain
{
    /// <summary>
    /// Chuyên nhận Hash và lưu lên Ethereum Sepolia Testnet.
    /// </summary>
    public interface IBlockchainService
    {
        /// <summary>
        /// Lưu file hash lên Smart Contract trên Sepolia.
        /// Trả về Transaction Hash.
        /// </summary>
        Task<string> StoreHashAsync(string fileHash, int entityId);

        /// <summary>
        /// Tính SHA-256 hash cho file.
        /// Trả về chuỗi hex có prefix "0x".
        /// </summary>
        Task<string> ComputeFileHashAsync(IFormFile file);
    }
}
