namespace AISEP.BLL.Services.Blockchain
{
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

        /// <summary>
        /// Gọi hàm verifyDocument(fileHash) trên Smart Contract (view, miễn phí gas).
        /// Trả về (startupId, timestamp). Nếu chưa được đăng ký thì timestamp = 0.
        /// </summary>
        Task<(int EntityId, long Timestamp)> VerifyDocumentAsync(string fileHash);
    }
}
