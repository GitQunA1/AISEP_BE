using AISEP.BLL.DTOs.Requests;
using AISEP.DAL.Enums;
using FluentValidation;

namespace AISEP.BLL.Validators.Startup
{
    public class CreateStartupRequestValidator : AbstractValidator<CreateStartupRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private static readonly string[] AllowedDocTypes = ["application/pdf", "image/jpeg", "image/png"];
        private const long MaxImageSize = 5 * 1024 * 1024;
        private const long MaxDocSize = 10 * 1024 * 1024;

        public CreateStartupRequestValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Tên công ty là bắt buộc.")
                .MaximumLength(255).WithMessage("Tên công ty không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Tên công ty chứa ký tự không hợp lệ.");

            RuleFor(x => x.Founder)
                .NotEmpty().WithMessage("Người sáng lập là bắt buộc.")
                .MaximumLength(255).WithMessage("Người sáng lập không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Người sáng lập chứa ký tự không hợp lệ.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email là bắt buộc.")
                .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự.")
                .EmailAddress().WithMessage("Email không đúng định dạng.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Số điện thoại là bắt buộc.")
                .MaximumLength(50).WithMessage("Số điện thoại không được vượt quá 50 ký tự.")
                .Matches("^(03|05|07|08|09)\\d{8}$").WithMessage("Số điện thoại phải bắt đầu bằng 03, 05, 07, 08 hoặc 09 và gồm 10 chữ số.");

            RuleFor(x => x.CountryCity)
                .NotEmpty().WithMessage("Quốc gia/Thành phố là bắt buộc.")
                .MaximumLength(255).WithMessage("Quốc gia/Thành phố không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Quốc gia/Thành phố chứa ký tự không hợp lệ.");

            RuleFor(x => x.Website)
                .MaximumLength(255).WithMessage("Website không được vượt quá 255 ký tự.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Website phải là URL hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.Website));

            RuleFor(x => x.Industry)
                .NotEmpty().WithMessage("Ngành nghề là bắt buộc.")
                .IsInEnum().WithMessage("Ngành nghề không hợp lệ.");

            RuleFor(x => x.LogoFile)
                .NotEmpty().WithMessage("File logo là bắt buộc.")
                .Must(f => f != null && f.Length <= MaxImageSize)
                .WithMessage("Logo không được vượt quá 5MB.")
                .Must(f => f != null && AllowedImageTypes.Contains(f.ContentType))
                .WithMessage("Logo chỉ hỗ trợ JPG, PNG, WEBP.");

            RuleFor(x => x.BusinessLicenseFile)
                .NotEmpty().WithMessage("File giấy phép kinh doanh là bắt buộc.")
                .Must(f => f != null && f.Length <= MaxDocSize)
                .WithMessage("Giấy phép kinh doanh không được vượt quá 10MB.")
                .Must(f => f != null && AllowedDocTypes.Contains(f.ContentType))
                .WithMessage("Giấy phép kinh doanh chỉ hỗ trợ PDF, JPG, PNG.");
        }
    }
}
