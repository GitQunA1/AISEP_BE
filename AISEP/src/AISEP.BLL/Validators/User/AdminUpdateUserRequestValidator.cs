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
                .WithMessage("At least one field must be provided for update.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("UserName must not be empty when provided.")
                .MinimumLength(2).WithMessage("UserName must be at least 2 characters.")
                .MaximumLength(100).WithMessage("UserName must not exceed 100 characters.")
                .When(x => x.UserName is not null);

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("FullName must not be empty when provided.")
                .MinimumLength(2).WithMessage("FullName must be at least 2 characters.")
                .MaximumLength(100).WithMessage("FullName must not exceed 100 characters.")
                .When(x => x.FullName is not null);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email must not be empty when provided.")
                .EmailAddress().WithMessage("Invalid email format.")
                .When(x => x.Email is not null);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("PhoneNumber must not exceed 20 characters.")
                .When(x => x.PhoneNumber is not null);

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Invalid role.")
                .When(x => x.Role.HasValue);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status.")
                .When(x => x.Status.HasValue);

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(_ => DateTime.UtcNow.Date)
                .WithMessage("Date of birth cannot be in the future.")
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
