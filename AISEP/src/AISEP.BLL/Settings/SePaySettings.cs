namespace AISEP.BLL.Settings
{
    public class SePaySettings
    {
        public string WebhookSecret { get; set; } = string.Empty;
        public string PaymentPrefix { get; set; } = "AISEP";

        // VietQR bank info
        public string BankCode { get; set; } = string.Empty;       // e.g. "MB", "VCB", "ACB"
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
    }
}
