using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Document
{
    public class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
    {
        private static readonly string[] AllowedMimeTypes = ["application/pdf", "image/jpeg", "image/png"];
        private const long MaxDocSize = 10 * 1024 * 1024; // 10 MB

        public UploadDocumentRequestValidator()
        {
            RuleFor(x => x.DocumentType)
                .IsInEnum().WithMessage("Document type is not valid. Allowed: PitchDeck, BusinessPlan, Other.");

            RuleFor(x => x.File)
                .NotNull().WithMessage("File is required.")
                .Must(f => f!.Length <= MaxDocSize)
                    .WithMessage("File must not exceed 10MB.")
                .Must(f => AllowedMimeTypes.Contains(f!.ContentType))
                    .WithMessage("File only supports PDF, JPG, PNG.")
                .When(x => x.File is not null);
        }
    }
}
