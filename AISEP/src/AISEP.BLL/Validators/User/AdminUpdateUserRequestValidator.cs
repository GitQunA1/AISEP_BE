using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.User
{
    public class AdminUpdateUserRequestValidator : AbstractValidator<AdminUpdateUserRequest>
    {
        public AdminUpdateUserRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("Cần cung cấp ít nhất một trường để cập nhật.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Tên người dùng không được để trống khi được cung cấp.")
                .MinimumLength(2).WithMessage("Tên người dùng phải có ít nhất 2 ký tự.")
                .MaximumLength(100).WithMessage("Tên người dùng không được vượt quá 100 ký tự.")
                .When(x => x.UserName is not null);

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ và tên không được để trống khi được cung cấp.")
                .MinimumLength(2).WithMessage("Họ và tên phải có ít nhất 2 ký tự.")
                .MaximumLength(100).WithMessage("Họ và tên không được vượt quá 100 ký tự.")
                .When(x => x.FullName is not null);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống khi được cung cấp.")
                .EmailAddress().WithMessage("Email không đúng định dạng.")
                .When(x => x.Email is not null);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự.")
                .When(x => x.PhoneNumber is not null);

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Vai trò không hợp lệ.")
                .When(x => x.Role.HasValue);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Trạng thái không hợp lệ.")
                .When(x => x.Status.HasValue);

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(_ => DateTime.UtcNow.Date)
                .WithMessage("Ngày sinh không được ở tương lai.")
                .When(x => x.DateOfBirth.HasValue);
        }

        private static bool HasAtLeastOneField(AdminUpdateUserRequest request)
        {
            return request.UserName is not null
                || request.FullName is not null
                || request.Email is not null
                || request.PhoneNumber is not null
                || request.Role.HasValue
                || request.Status.HasValue
                || request.IsPremium.HasValue
                || request.DateOfBirth.HasValue;
        }
    }
}
