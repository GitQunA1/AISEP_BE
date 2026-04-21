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
                .WithMessage("Cần cung cấp ít nhất một trường để cập nhật.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Tên người dùng không được để trống khi được cung cấp.")
                .Matches(NamePattern).WithMessage("Tên người dùng chỉ được chứa chữ cái và khoảng trắng.")
                .When(x => x.UserName is not null);

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ và tên không được để trống khi được cung cấp.")
                .Matches(NamePattern).WithMessage("Họ và tên chỉ được chứa chữ cái và khoảng trắng.")
                .When(x => x.FullName is not null);

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(_ => DateTime.UtcNow.Date)
                .WithMessage("Ngày sinh không được ở tương lai.")
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
