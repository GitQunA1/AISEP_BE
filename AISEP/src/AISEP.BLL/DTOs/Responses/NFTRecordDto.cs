namespace AISEP.BLL.DTOs.Responses
{
    public class NFTRecordDto
    {
        public int NFTRecordId { get; set; }
        public int DealId { get; set; }
        public string TokenId { get; set; } = string.Empty;
        public string TxHash { get; set; } = string.Empty;
        public string OwnerWallet { get; set; } = string.Empty;
        public DateTime MintedAt { get; set; }
        public string ValidityStatus { get; set; } = string.Empty;
        public bool Transferable { get; set; }
        public string? PreviousOwnerWallet { get; set; }
    }
}
