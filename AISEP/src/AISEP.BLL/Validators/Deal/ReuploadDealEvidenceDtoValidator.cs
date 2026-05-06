using AISEP.BLL.DTOs.Requests;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace AISEP.BLL.Validators.Deal
{
    public class ReuploadDealEvidenceDtoValidator : AbstractValidator<ReuploadDealEvidenceDto>
    {
        public ReuploadDealEvidenceDtoValidator()
        {
            //RuleFor(x => x.EvidenceFile)

            //    .NotNull().WithMessage("Tệp bằng chứng là bắt buộc.")
            //    .Must(file => file is { Length: > 0 }).WithMessage("Tệp bằng chứng là bắt buộc.");

            RuleFor(x => x.EvidenceFile)
                .Must(IsSupportedEvidence)
                .WithMessage("Tệp bằng chứng phải là hình ảnh hoặc PDF.")

                .NotNull().WithMessage("Tệp bằng chứng là bắt buộc.")
                .Must(file => file is { Length: > 0 }).WithMessage("Tệp bằng chứng là bắt buộc.");

            //RuleFor(x => x.EvidenceFile)
            //    .Must(IsSupportedEvidence)
            //    .WithMessage("EvidenceFile phải là ảnh hoặc PDF.")

            //    .When(x => x.EvidenceFile is not null);
        }

        private static bool IsSupportedEvidence(IFormFile file)
        {
            var contentType = file.ContentType?.Trim().ToLowerInvariant() ?? string.Empty;
            var extension = Path.GetExtension(file.FileName).Trim().ToLowerInvariant();

            if (contentType.StartsWith("image/"))
            {
                return true;
            }

            if (contentType == "application/pdf" || extension == ".pdf")
            {
                return true;
            }

            return extension is ".jpg" or ".jpeg" or ".png" or ".webp";
        }
    }
}
