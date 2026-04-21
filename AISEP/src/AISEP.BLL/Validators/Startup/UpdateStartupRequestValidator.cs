using AISEP.BLL.DTOs.Requests;
using AISEP.DAL.Enums;
using FluentValidation;

namespace AISEP.BLL.Validators.Startup
{
    public class UpdateStartupRequestValidator : AbstractValidator<UpdateStartupRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private static readonly string[] AllowedDocTypes = ["application/pdf", "image/jpeg", "image/png"];
        private const long MaxImageSize = 5 * 1024 * 1024;
        private const long MaxDocSize = 10 * 1024 * 1024;

        public UpdateStartupRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("Cần cung cấp ít nhất một trường để cập nhật.");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Tên công ty không được để trống khi được cung cấp.")
                .MaximumLength(255).WithMessage("Tên công ty không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Tên công ty chứa ký tự không hợp lệ.")
                .When(x => x.CompanyName is not null);

            RuleFor(x => x.Founder)
                .NotEmpty().WithMessage("Người sáng lập không được để trống khi được cung cấp.")
                .MaximumLength(255).WithMessage("Người sáng lập không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Người sáng lập chứa ký tự không hợp lệ.")
                .When(x => x.Founder is not null);

            RuleFor(x => x.CountryCity)
                .NotEmpty().WithMessage("Quốc gia/Thành phố không được để trống khi được cung cấp.")
                .MaximumLength(255).WithMessage("Quốc gia/Thành phố không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Quốc gia/Thành phố chứa ký tự không hợp lệ.")
                .When(x => x.CountryCity is not null);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống khi được cung cấp.")
                .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự.")
                .EmailAddress().WithMessage("Email không đúng định dạng.")
                .When(x => x.Email is not null);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Số điện thoại không được để trống khi được cung cấp.")
                .MaximumLength(50).WithMessage("Số điện thoại không được vượt quá 50 ký tự.")
                .Matches("^(03|05|07|08|09)\\d{8}$").WithMessage("Số điện thoại phải bắt đầu bằng 03, 05, 07, 08 hoặc 09 và gồm 10 chữ số.")
                .When(x => x.PhoneNumber is not null);

            RuleFor(x => x.Website)
                .MaximumLength(255).WithMessage("Website không được vượt quá 255 ký tự.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Website phải là URL hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.Website));

            RuleFor(x => x.Industry)
                .IsInEnum().WithMessage("Ngành nghề không hợp lệ.")
                .When(x => x.Industry.HasValue);

            RuleFor(x => x.LogoFile)
                .Must(f => f!.Length <= MaxImageSize)
                .WithMessage("Logo không được vượt quá 5MB.")
                .Must(f => AllowedImageTypes.Contains(f!.ContentType))
                .WithMessage("Logo chỉ hỗ trợ JPG, PNG, WEBP.")
                .When(x => x.LogoFile is not null);

            RuleFor(x => x.BusinessLicenseFile)
                .Must(f => f!.Length <= MaxDocSize)
                .WithMessage("Giấy phép kinh doanh không được vượt quá 10MB.")
                .Must(f => AllowedDocTypes.Contains(f!.ContentType))
                .WithMessage("Giấy phép kinh doanh chỉ hỗ trợ PDF, JPG, PNG.")
                .When(x => x.BusinessLicenseFile is not null);
        }

        private static bool HasAtLeastOneField(UpdateStartupRequest request)
        {
            return request.CompanyName is not null
                || request.Founder is not null
                || request.Email is not null
                || request.PhoneNumber is not null
                || request.CountryCity is not null
                || request.Website is not null
                || request.Industry.HasValue
                || request.LogoFile is not null
                || request.BusinessLicenseFile is not null;
        }
    }
}
