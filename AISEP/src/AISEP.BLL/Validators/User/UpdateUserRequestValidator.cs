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
        public UpdateUserRequestValidator() 
        {
            RuleFor(x => x.UserName)
                .Matches("^[a-zA-ZÀ-ỹ\\s]+$").WithMessage("User name must only contain letters and spaces.");
                //.When(x => !string.IsNullOrWhiteSpace(x.UserName));
        }
    }
}
