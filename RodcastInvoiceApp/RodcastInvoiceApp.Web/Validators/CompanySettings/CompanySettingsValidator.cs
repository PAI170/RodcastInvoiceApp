using FluentValidation;
using Microsoft.Extensions.Localization;
using RodcastInvoiceApp.Web.DataTransferObjects.CompanySettings;
using RodcastInvoiceApp.Web.Resources;

namespace RodcastInvoiceApp.Web.Validators.CompanySettings
{
    public class CompanySettingsValidator : AbstractValidator<CompanySettingsDto>
    {
        public CompanySettingsValidator(IStringLocalizer<SharedResource> loc)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(loc["Val_Required_CompanyName"])
                .MaximumLength(150).WithMessage(loc["Val_MaxLength", 150]);

            RuleFor(x => x.TaxId)
                .NotEmpty().WithMessage(loc["Val_Required_TaxId"])
                .MaximumLength(50).WithMessage(loc["Val_MaxLength", 50]);

            RuleFor(x => x.Address)
                .MaximumLength(250).WithMessage(loc["Val_MaxLength", 250]);
        }
    }
}
