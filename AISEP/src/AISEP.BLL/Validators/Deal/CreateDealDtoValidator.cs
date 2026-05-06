using AISEP.BLL.DTOs.Requests;
using AISEP.DAL.Enums;
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

            RuleFor(x => x.InvestedAmount)
                .GreaterThan(0).WithMessage("InvestedAmount phải lớn hơn 0.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Type không hợp lệ.");

            RuleFor(x => x.EquityPercentage)
                .NotNull().WithMessage("EquityPercentage là bắt buộc khi chọn Cổ phần.")
                .GreaterThan(0).WithMessage("EquityPercentage phải lớn hơn 0.")
                .LessThanOrEqualTo(100).WithMessage("EquityPercentage phải nhỏ hơn hoặc bằng 100.")
                .When(x => x.Type == InvestmentType.Equity);

            RuleFor(x => x.ExchangeTerms)
                .NotEmpty().WithMessage("ExchangeTerms là bắt buộc khi chọn Điều khoản khác.")
                .MaximumLength(500).WithMessage("ExchangeTerms tối đa 500 ký tự.")
                .When(x => x.Type == InvestmentType.CustomTerms);

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
