using AISEP.BLL.DTOs.Requests;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using AppUser = AISEP.DAL.Entities.User;

namespace AISEP.BLL.Validators.Auth
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s]*$";
        //private readonly UserManager<AppUser> _userManager;

        public RegisterRequestValidator() { 
           // _userManager = userManager;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
                .Matches(TextPattern).WithMessage("Name contains invalid characters.");



            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Fullname is required.")
                .MinimumLength(2).WithMessage("Fullname must be at least 2 characters.")
                .MaximumLength(100).WithMessage("Fullname must not exceed 100 characters.")
                .Matches(TextPattern).WithMessage("Fullname contains invalid characters.");
                

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
                .WithMessage("Password must contain uppercase, lowercase, number and special character.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.Password).WithMessage("Passwords do not match.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Invalid role.");
        }

        
    }
}
