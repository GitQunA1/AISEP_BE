namespace AISEP.BLL.Services.PdfExtraction
{
    public interface IPdfExtractionService
    {
        Task<string> ExtractTextFromPdfUrlAsync(string pdfUrl);
    }
}
