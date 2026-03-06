namespace AISEP.DTOs.Responses
{
    public class BlockchainVerificationResponse
    {
        public bool IsAuthentic { get; set; } // Trùng khớp hash hay không?
        public string TxHash { get; set; } = string.Empty; // Mã giao dịch để user click vào xem trên Etherscan
        public string TimestampOnBlockchain { get; set; } = string.Empty; // Thời gian ghi nhận bất biến
        public string Message { get; set; } = string.Empty; // "Tài liệu chưa bị chỉnh sửa và đã được bảo vệ"
    }
}
