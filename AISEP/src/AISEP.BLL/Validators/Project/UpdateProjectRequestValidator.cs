using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Project
{
    public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";
        private const string TeamMembersPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxImageSize = 5 * 1024 * 1024;

        public UpdateProjectRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("Cần cung cấp ít nhất một trường để cập nhật.");

            RuleFor(x => x.ProjectName)
                .NotEmpty().WithMessage("Tên dự án không được để trống khi được cung cấp.")
                .MaximumLength(255).WithMessage("Tên dự án không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Tên dự án chứa ký tự không hợp lệ.")
                .When(x => x.ProjectName is not null);

            RuleFor(x => x.ProjectImageFile)
                .Must(f => f!.Length <= MaxImageSize)
                .WithMessage("Ảnh dự án không được vượt quá 5MB.")
                .Must(f => AllowedImageTypes.Contains(f!.ContentType))
                .WithMessage("Ảnh dự án chỉ hỗ trợ JPG, PNG, WEBP.")
                .When(x => x.ProjectImageFile is not null);

            RuleFor(x => x.DevelopmentStage)
                .IsInEnum().WithMessage("Giai đoạn phát triển không hợp lệ.")
                .When(x => x.DevelopmentStage.HasValue);

            RuleFor(x => x.Industry)
                .IsInEnum().WithMessage("Ngành nghề không hợp lệ.")
                .When(x => x.Industry.HasValue);

            RuleFor(x => x.ShortDescription)
                .NotEmpty().WithMessage("Mô tả ngắn không được để trống khi được cung cấp.")
                .MaximumLength(500).WithMessage("Mô tả ngắn không được vượt quá 500 ký tự.")
                .Matches(TextPattern).WithMessage("Mô tả ngắn chứa ký tự không hợp lệ.")
                .When(x => x.ShortDescription is not null);

            RuleFor(x => x.ProblemStatement)
                .NotEmpty().WithMessage("Mô tả vấn đề không được để trống khi được cung cấp.")
                .MaximumLength(2000).WithMessage("Mô tả vấn đề không được vượt quá 2000 ký tự.")
                .Matches(TextPattern).WithMessage("Mô tả vấn đề chứa ký tự không hợp lệ.")
                .When(x => x.ProblemStatement is not null);

            RuleFor(x => x.SolutionDescription)
                .NotEmpty().WithMessage("Mô tả giải pháp không được để trống khi được cung cấp.")
                .MaximumLength(2000).WithMessage("Mô tả giải pháp không được vượt quá 2000 ký tự.")
                .Matches(TextPattern).WithMessage("Mô tả giải pháp chứa ký tự không hợp lệ.")
                .When(x => x.SolutionDescription is not null);

            RuleFor(x => x.TargetCustomers)
                .NotEmpty().WithMessage("Khách hàng mục tiêu không được để trống khi được cung cấp.")
                .MaximumLength(1000).WithMessage("Khách hàng mục tiêu không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Khách hàng mục tiêu chứa ký tự không hợp lệ.")
                .When(x => x.TargetCustomers is not null);

            RuleFor(x => x.UniqueValueProposition)
                .NotEmpty().WithMessage("Giá trị khác biệt không được để trống khi được cung cấp.")
                .MaximumLength(1000).WithMessage("Giá trị khác biệt không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Giá trị khác biệt chứa ký tự không hợp lệ.")
                .When(x => x.UniqueValueProposition is not null);

            RuleFor(x => x.MarketSize)
                .GreaterThanOrEqualTo(0).WithMessage("Quy mô thị trường phải lớn hơn hoặc bằng 0.")
                .When(x => x.MarketSize.HasValue);

            RuleFor(x => x.BusinessModel)
                .NotEmpty().WithMessage("Mô hình kinh doanh không được để trống khi được cung cấp.")
                .MaximumLength(1000).WithMessage("Mô hình kinh doanh không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Mô hình kinh doanh chứa ký tự không hợp lệ.")
                .When(x => x.BusinessModel is not null);

            RuleFor(x => x.Revenue)
                .GreaterThanOrEqualTo(0).WithMessage("Doanh thu phải lớn hơn hoặc bằng 0.")
                .When(x => x.Revenue.HasValue);

            RuleFor(x => x.Competitors)
                .NotEmpty().WithMessage("Đối thủ cạnh tranh không được để trống khi được cung cấp.")
                .MaximumLength(1000).WithMessage("Đối thủ cạnh tranh không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Đối thủ cạnh tranh chứa ký tự không hợp lệ.")
                .When(x => x.Competitors is not null);

            RuleFor(x => x.TeamMembers)
                .NotEmpty().WithMessage("Thành viên đội ngũ không được để trống khi được cung cấp.")
                .MaximumLength(1000).WithMessage("Thành viên đội ngũ không được vượt quá 1000 ký tự.")
                .Matches(TeamMembersPattern).WithMessage("Thành viên đội ngũ chứa ký tự không hợp lệ.")
                .When(x => x.TeamMembers is not null);

            RuleFor(x => x.KeySkills)
                .NotEmpty().WithMessage("Kỹ năng chính không được để trống khi được cung cấp.")
                .MaximumLength(1000).WithMessage("Kỹ năng chính không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Kỹ năng chính chứa ký tự không hợp lệ.")
                .When(x => x.KeySkills is not null);

            RuleFor(x => x.TeamExperience)
                .NotEmpty().WithMessage("Kinh nghiệm đội ngũ không được để trống khi được cung cấp.")
                .MaximumLength(1000).WithMessage("Kinh nghiệm đội ngũ không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Kinh nghiệm đội ngũ chứa ký tự không hợp lệ.")
                .When(x => x.TeamExperience is not null);
        }

        private static bool HasAtLeastOneField(UpdateProjectRequest request)
        {
            return request.ProjectName is not null
                || request.ProjectImageFile is not null
                || request.ShortDescription is not null
                || request.DevelopmentStage.HasValue
                || request.ProblemStatement is not null
                || request.SolutionDescription is not null
                || request.TargetCustomers is not null
                || request.UniqueValueProposition is not null
                || request.MarketSize.HasValue
                || request.BusinessModel is not null
                || request.Revenue.HasValue
                || request.Competitors is not null
                || request.TeamMembers is not null
                || request.KeySkills is not null
                || request.TeamExperience is not null
                || request.Industry.HasValue;
        }
    }
}
