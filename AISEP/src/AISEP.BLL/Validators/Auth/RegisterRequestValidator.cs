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
        private const string FullNamePattern = @"^[\p{L}\s]*$";
        //private readonly UserManager<AppUser> _userManager;

        public RegisterRequestValidator() { 
           // _userManager = userManager;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên người dùng là bắt buộc.")
                .MinimumLength(2).WithMessage("Tên người dùng phải có ít nhất 2 ký tự.")
                .MaximumLength(100).WithMessage("Tên người dùng không được vượt quá 100 ký tự.")
                .Matches(TextPattern).WithMessage("Tên người dùng chứa ký tự không hợp lệ.");



            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ và tên là bắt buộc.")
                .MinimumLength(2).WithMessage("Họ và tên phải có ít nhất 2 ký tự.")
                .MaximumLength(100).WithMessage("Họ và tên không được vượt quá 100 ký tự.")
                .Matches(TextPattern).WithMessage("Họ và tên chứa ký tự không hợp lệ.");
                

            RuleFor(x => x.FullName)
                .Matches(FullNamePattern).WithMessage("Họ và tên chỉ được chứa chữ cái và khoảng trắng.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email là bắt buộc.")
                .EmailAddress().WithMessage("Email không đúng định dạng.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu là bắt buộc.")
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
                .WithMessage("Mật khẩu phải chứa chữ hoa, chữ thường, số và ký tự đặc biệt.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Xác nhận mật khẩu là bắt buộc.")
                .Equal(x => x.Password).WithMessage("Mật khẩu xác nhận không khớp.");

            RuleFor(x => x.IsTermsAccepted)
                .Equal(true).WithMessage("Bạn phải đồng ý điều khoản sử dụng.");

            RuleFor(x => x.TermsVersion)
                .NotEmpty().WithMessage("Phiên bản điều khoản là bắt buộc.")
                .MaximumLength(50).WithMessage("Phiên bản điều khoản không được vượt quá 50 ký tự.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Vai trò không hợp lệ.");
        }

        
    }
}
