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
                .NotEmpty().WithMessage("UserName là bắt buộc.")
                .MinimumLength(2).WithMessage("UserName phải có ít nhất 2 ký tự.")
                .MaximumLength(100).WithMessage("UserName không được vượt quá 100 ký tự.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("FullName là bắt buộc.")
                .MinimumLength(2).WithMessage("FullName phải có ít nhất 2 ký tự.")
                .MaximumLength(100).WithMessage("FullName không được vượt quá 100 ký tự.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email là bắt buộc.")
                .EmailAddress().WithMessage("Email không đúng định dạng.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("PhoneNumber không được vượt quá 20 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu là bắt buộc.")
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
                .Matches(PasswordPattern)
                .WithMessage("Mật khẩu phải chứa chữ hoa, chữ thường, số và ký tự đặc biệt.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Xác nhận mật khẩu là bắt buộc.")
                .Equal(x => x.Password).WithMessage("Mật khẩu không khớp.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Trạng thái không hợp lệ.");

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(_ => DateTime.UtcNow.Date)
                .WithMessage("Ngày sinh không được nằm trong tương lai.")
                .When(x => x.DateOfBirth.HasValue);
        }
    }
}
