using System.Text;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace AISEP.BLL.Services.PdfExtraction
{
    public class PdfExtractionService : IPdfExtractionService
    {
        private const int MaxPagesToRead = 20;
        private const int MaxExtractedCharacters = 15_000;
        private const string TruncatedSuffix = "... [Đã cắt bớt do quá dài]";

        private readonly HttpClient _httpClient;
        private readonly ILogger<PdfExtractionService> _logger;

        public PdfExtractionService(HttpClient httpClient, ILogger<PdfExtractionService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> ExtractTextFromPdfUrlAsync(string pdfUrl)
        {
            if (string.IsNullOrWhiteSpace(pdfUrl))
            {
                return string.Empty;
            }

            try
            {
                using var response = await _httpClient.GetAsync(pdfUrl, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Cannot download PDF from {PdfUrl}. StatusCode={StatusCode}", pdfUrl, response.StatusCode);
                    return string.Empty;
                }

                await using var sourceStream = await response.Content.ReadAsStreamAsync();
                using var memoryStream = new MemoryStream();
                await sourceStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var builder = new StringBuilder();
                using var document = PdfDocument.Open(memoryStream);

                foreach (var page in document.GetPages().Take(MaxPagesToRead))
                {
                    if (builder.Length >= MaxExtractedCharacters)
                    {
                        break;
                    }

                    builder.AppendLine(page.Text);
                    builder.AppendLine();
                }

                return LimitLength(builder.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cannot extract text from PDF url {PdfUrl}", pdfUrl);
                return string.Empty;
            }
        }

        private static string LimitLength(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var normalized = text.Trim();
            if (normalized.Length <= MaxExtractedCharacters)
            {
                return normalized;
            }

            var allowedLength = Math.Max(0, MaxExtractedCharacters - TruncatedSuffix.Length);
            return normalized[..allowedLength].TrimEnd() + TruncatedSuffix;
        }
    }
}
