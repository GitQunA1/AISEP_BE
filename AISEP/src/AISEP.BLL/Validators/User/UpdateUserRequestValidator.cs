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
            RuleFor(x => x.UserName)
                .Matches(NamePattern).WithMessage("User name must only contain letters and spaces.")
                .When(x => !string.IsNullOrWhiteSpace(x.UserName));

            RuleFor(x => x.FullName)
                .Matches(NamePattern).WithMessage("Full name must only contain letters and spaces.")
                .When(x => !string.IsNullOrWhiteSpace(x.FullName));

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(_ => DateTime.UtcNow.Date)
                .WithMessage("Date of birth cannot be in the future.")
                .When(x => x.DateOfBirth.HasValue);
        }
    }
}
