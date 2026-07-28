using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Localization;
using RodcastInvoiceApp.Web.DataTransferObjects.Project;
using RodcastInvoiceApp.Web.Resources;

namespace RodcastInvoiceApp.Web.Validators.Project
{
    public class ProjectUpdateValidator : AbstractValidator<ProjectUpdateDto>
    {
        public ProjectUpdateValidator(IStringLocalizer<SharedResource> loc)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(loc["Val_Required_ProjectName"])
                .MaximumLength(150).WithMessage(loc["Val_MaxLength", 150]);

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage(loc["Val_Required_ProjectCode"])
                .MaximumLength(50).WithMessage(loc["Val_MaxLength", 50]);

            RuleFor(x => x.CostCenter)
                .MaximumLength(50).WithMessage(loc["Val_MaxLength", 50])
                .When(x => !string.IsNullOrWhiteSpace(x.CostCenter));

            RuleFor(x => x.BillingType)
                .IsInEnum().WithMessage(loc["Val_Invalid_BillingType"]);

            RuleFor(x => x.Config)
                .Must(BeValidJson).WithMessage(loc["Val_Invalid_Json"]);
        }

        private static bool BeValidJson(string config)
        {
            if (string.IsNullOrWhiteSpace(config)) return false;
            try
            {
                JsonDocument.Parse(config);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
