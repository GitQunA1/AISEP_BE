using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Document
{
    public class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
    {
        private const long MaxDocSize = 10 * 1024 * 1024; // 10 MB

        public UploadDocumentRequestValidator()
        {
            RuleFor(x => x.DocumentType)
                .IsInEnum().WithMessage("Loại tài liệu không hợp lệ.");

            RuleFor(x => x.File)
                .NotNull().WithMessage("Tệp là bắt buộc.")
                .Must(f => f!.Length <= MaxDocSize)
                    .WithMessage("Tệp không được vượt quá 10MB.")
                .Must(IsPdfFile)
                    .WithMessage("Tệp chỉ hỗ trợ định dạng PDF.")
                .When(x => x.File is not null);
        }

        private static bool IsPdfFile(IFormFile? file)
        {
            if (file is null)
            {
                return false;
            }

            var contentType = file.ContentType?.Trim().ToLowerInvariant();
            var extension = Path.GetExtension(file.FileName).Trim().ToLowerInvariant();
            return contentType == "application/pdf" && extension == ".pdf";
        }
    }
}
