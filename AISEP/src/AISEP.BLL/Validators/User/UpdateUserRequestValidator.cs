using AISEP.BLL.DTOs.Requests;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISEP.BLL.Validators.User
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        private const string NamePattern = @"^[\p{L}\p{N}\s]+$";
        public UpdateUserRequestValidator() 
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("At least one field must be provided for update.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("User name must not be empty when provided.")
                .Matches(NamePattern).WithMessage("User name must only contain letters and spaces.")
                .When(x => x.UserName is not null);

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name must not be empty when provided.")
                .Matches(NamePattern).WithMessage("Full name must only contain letters and spaces.")
                .When(x => x.FullName is not null);

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(_ => DateTime.UtcNow.Date)
                .WithMessage("Date of birth cannot be in the future.")
                .When(x => x.DateOfBirth.HasValue);
        }

        private static bool HasAtLeastOneField(UpdateUserRequest request)
        {
            return request.UserName is not null
                || request.FullName is not null
                || request.DateOfBirth.HasValue;
        }
    }
}
