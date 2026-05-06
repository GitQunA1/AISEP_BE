using AISEP.BLL.DTOs.Requests;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace AISEP.BLL.Validators.Deal
{
    public class CreateDealDtoValidator : AbstractValidator<CreateDealDto>
    {
        public CreateDealDtoValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty().WithMessage("Mã dự án là bắt buộc.")
                .GreaterThan(0).WithMessage("Mã dự án phải là số dương.");

            RuleFor(x => x.EvidenceFile)
                .NotNull().WithMessage("Tệp bằng chứng là bắt buộc.")
                .Must(file => file is { Length: > 0 }).WithMessage("Tệp bằng chứng là bắt buộc.");

            RuleFor(x => x.EvidenceFile)
                .Must(IsSupportedEvidence)
                .WithMessage("Tệp bằng chứng phải là hình ảnh hoặc PDF.")
                .When(x => x.EvidenceFile is not null);
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
