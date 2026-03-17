using AISEP.BLL.DTOs.Requests;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISEP.BLL.Validators.Advisor
{
    public class RejectAdvisorValidator : AbstractValidator<RejectAdvisorRequest>
    {
        public RejectAdvisorValidator()
        {
            RuleFor(x => x.Reason)
                .NotNull().WithMessage("Reason must not be null.")
               //.Must(reason => !string.IsNullOrWhiteSpace(reason)).WithMessage("Reason is required.")
               .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Reason must not contains invalid characters.")
                .MaximumLength(2000).WithMessage("Reason must not exceed 2000 characters.");
        }
    }
}
