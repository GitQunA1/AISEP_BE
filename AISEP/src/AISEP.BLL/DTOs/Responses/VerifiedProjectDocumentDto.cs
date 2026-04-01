namespace AISEP.BLL.DTOs.Responses
{
    public class VerifiedProjectDocumentDto
    {
        public int DocumentId { get; set; }
        public string TxHash { get; set; } = string.Empty;
        public string TimestampOnBlockchain { get; set; } = string.Empty;
        public string SignerAddress { get; set; } = string.Empty;
    }
}
