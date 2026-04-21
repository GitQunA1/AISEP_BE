using AISEP.BLL.DTOs.Requests;
using AISEP.DAL.Enums;
using FluentValidation;

namespace AISEP.BLL.Validators.Project
{
    public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";
        private const string TeamMembersPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxImageSize = 5 * 1024 * 1024;

        public CreateProjectRequestValidator()
        {
            RuleFor(x => x.DevelopmentStage)
                .IsInEnum().WithMessage("Giai đoạn phát triển không hợp lệ.");

            RuleFor(x => x.Industry)
                .NotNull().WithMessage("Ngành nghề là bắt buộc.")
                .IsInEnum().WithMessage("Ngành nghề không hợp lệ.");

            RuleFor(x => x.ProjectName)
                .NotEmpty().WithMessage("Tên dự án là bắt buộc.")
                .MaximumLength(255).WithMessage("Tên dự án không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Tên dự án chứa ký tự không hợp lệ.");

            RuleFor(x => x.ProjectImageFile)
                .Must(f => f!.Length <= MaxImageSize)
                .WithMessage("Ảnh dự án không được vượt quá 5MB.")
                .Must(f => AllowedImageTypes.Contains(f!.ContentType))
                .WithMessage("Ảnh dự án chỉ hỗ trợ JPG, PNG, WEBP.")
                .When(x => x.ProjectImageFile is not null);

            RuleFor(x => x.ShortDescription)
                .NotEmpty().WithMessage("Mô tả ngắn là bắt buộc.")
                .MaximumLength(500).WithMessage("Mô tả ngắn không được vượt quá 500 ký tự.")
                .Matches(TextPattern).WithMessage("Mô tả ngắn chứa ký tự không hợp lệ.");

            RuleFor(x => x.ProblemStatement)
                .NotEmpty().WithMessage("Vấn đề dự án giải quyết là bắt buộc.")
                .MaximumLength(2000).WithMessage("Mô tả vấn đề không được vượt quá 2000 ký tự.")
                .Matches(TextPattern).WithMessage("Mô tả vấn đề chứa ký tự không hợp lệ.");

            RuleFor(x => x.SolutionDescription)
                .NotEmpty().WithMessage("Mô tả giải pháp là bắt buộc.")
                .MaximumLength(2000).WithMessage("Mô tả giải pháp không được vượt quá 2000 ký tự.")
                .Matches(TextPattern).WithMessage("Mô tả giải pháp chứa ký tự không hợp lệ.");

            RuleFor(x => x.TargetCustomers)
                .NotEmpty().WithMessage("Khách hàng mục tiêu là bắt buộc.")
                .MaximumLength(1000).WithMessage("Khách hàng mục tiêu không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Khách hàng mục tiêu chứa ký tự không hợp lệ.");

            RuleFor(x => x.TeamMembers)
                .NotEmpty().WithMessage("Thành viên đội ngũ là bắt buộc.")
                .MaximumLength(1000).WithMessage("Thành viên đội ngũ không được vượt quá 1000 ký tự.")
                .Matches(TeamMembersPattern).WithMessage("Thành viên đội ngũ chứa ký tự không hợp lệ.");

            RuleFor(x => x.UniqueValueProposition)
                .MaximumLength(1000).WithMessage("Giá trị khác biệt không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Giá trị khác biệt chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.UniqueValueProposition));

            RuleFor(x => x.BusinessModel)
                .MaximumLength(1000).WithMessage("Mô hình kinh doanh không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Mô hình kinh doanh chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.BusinessModel));

            RuleFor(x => x.KeySkills)
                .MaximumLength(1000).WithMessage("Kỹ năng chính không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Kỹ năng chính chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.KeySkills));

            RuleFor(x => x.Competitors)
                .MaximumLength(1000).WithMessage("Đối thủ cạnh tranh không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Đối thủ cạnh tranh chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.Competitors));

            RuleFor(x => x.TeamExperience)
                .MaximumLength(2000).WithMessage("Kinh nghiệm đội ngũ không được vượt quá 2000 ký tự.")
                .Matches(TextPattern).WithMessage("Kinh nghiệm đội ngũ chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.TeamExperience));

            RuleFor(x => x.Revenue)
                .GreaterThanOrEqualTo(0).WithMessage("Doanh thu phải lớn hơn hoặc bằng 0.")
                .When(x => x.Revenue.HasValue);

            RuleFor(x => x.MarketSize)
                .GreaterThanOrEqualTo(0).WithMessage("Quy mô thị trường phải lớn hơn hoặc bằng 0.")
                .When(x => x.MarketSize.HasValue);

            When(IsMvpOrGrowth, () =>
            {
                RuleFor(x => x.UniqueValueProposition)
                    .NotEmpty().WithMessage("Giá trị khác biệt là bắt buộc với giai đoạn MVP và Growth.");

                RuleFor(x => x.BusinessModel)
                    .NotEmpty().WithMessage("Mô hình kinh doanh là bắt buộc với giai đoạn MVP và Growth.");

                RuleFor(x => x.KeySkills)
                    .NotEmpty().WithMessage("Kỹ năng chính là bắt buộc với giai đoạn MVP và Growth.");

                RuleFor(x => x.Competitors)
                    .NotEmpty().WithMessage("Đối thủ cạnh tranh là bắt buộc với giai đoạn MVP và Growth.");
            });

            When(x => x.DevelopmentStage == DevelopmentStage.Growth, () =>
            {
                RuleFor(x => x.Revenue)
                    .NotNull().WithMessage("Doanh thu là bắt buộc với giai đoạn Growth.")
                    .GreaterThan(0).WithMessage("Doanh thu phải lớn hơn 0 với giai đoạn Growth.");

                RuleFor(x => x.MarketSize)
                    .NotNull().WithMessage("Quy mô thị trường là bắt buộc với giai đoạn Growth.")
                    .GreaterThan(0).WithMessage("Quy mô thị trường phải lớn hơn 0 với giai đoạn Growth.");

                RuleFor(x => x.TeamExperience)
                    .NotEmpty().WithMessage("Kinh nghiệm đội ngũ là bắt buộc với giai đoạn Growth.");
            });
        }

        private static bool IsMvpOrGrowth(CreateProjectRequest request)
        {
            return request.DevelopmentStage is DevelopmentStage.MVP or DevelopmentStage.Growth;
        }
    }
}
