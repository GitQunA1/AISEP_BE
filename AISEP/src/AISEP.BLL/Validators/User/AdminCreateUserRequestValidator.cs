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
                .NotEmpty().WithMessage("UserName is required.")
                .MinimumLength(2).WithMessage("UserName must be at least 2 characters.")
                .MaximumLength(100).WithMessage("UserName must not exceed 100 characters.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("FullName is required.")
                .MinimumLength(2).WithMessage("FullName must be at least 2 characters.")
                .MaximumLength(100).WithMessage("FullName must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("PhoneNumber must not exceed 20 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches(PasswordPattern)
                .WithMessage("Password must contain uppercase, lowercase, number and special character.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("ConfirmPassword is required.")
                .Equal(x => x.Password).WithMessage("Passwords do not match.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status.");

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(_ => DateTime.UtcNow.Date)
                .WithMessage("Date of birth cannot be in the future.")
                .When(x => x.DateOfBirth.HasValue);
        }
    }
}
