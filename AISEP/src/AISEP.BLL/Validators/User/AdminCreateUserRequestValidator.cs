using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.User
{
    public class AdminCreateUserRequestValidator : AbstractValidator<AdminCreateUserRequest>
    {
        private const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";

        public AdminCreateUserRequestValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Tên người dùng là bắt buộc.")
                .MinimumLength(2).WithMessage("Tên người dùng phải có ít nhất 2 ký tự.")
                .MaximumLength(100).WithMessage("Tên người dùng không được vượt quá 100 ký tự.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ và tên là bắt buộc.")
                .MinimumLength(2).WithMessage("Họ và tên phải có ít nhất 2 ký tự.")
                .MaximumLength(100).WithMessage("Họ và tên không được vượt quá 100 ký tự.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email là bắt buộc.")
                .EmailAddress().WithMessage("Email không đúng định dạng.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu là bắt buộc.")
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
                .Matches(PasswordPattern)
                .WithMessage("Mật khẩu phải chứa chữ hoa, chữ thường, số và ký tự đặc biệt.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Xác nhận mật khẩu là bắt buộc.")
                .Equal(x => x.Password).WithMessage("Mật khẩu xác nhận không khớp.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Trạng thái không hợp lệ.");

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(_ => DateTime.UtcNow.Date)
                .WithMessage("Ngày sinh không được ở tương lai.")
                .When(x => x.DateOfBirth.HasValue);
        }
    }
}
