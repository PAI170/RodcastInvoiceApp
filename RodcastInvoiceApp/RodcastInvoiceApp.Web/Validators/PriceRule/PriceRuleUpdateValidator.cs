using FluentValidation;
using Microsoft.Extensions.Localization;
using RodcastInvoiceApp.Web.DataTransferObjects.PriceRule;
using RodcastInvoiceApp.Web.Resources;

namespace RodcastInvoiceApp.Web.Validators.PriceRule
{
    public class PriceRuleUpdateValidator : AbstractValidator<PriceRuleUpdateDto>
    {
        public PriceRuleUpdateValidator(IStringLocalizer<SharedResource> loc)
        {
            RuleFor(x => x.Dimension1)
                .NotEmpty().WithMessage(loc["Val_Dimension1_Required"])
                .MaximumLength(100).WithMessage(loc["Val_MaxLength", 100]);

            RuleFor(x => x.Dimension2)
                .MaximumLength(100).WithMessage(loc["Val_MaxLength", 100])
                .When(x => !string.IsNullOrWhiteSpace(x.Dimension2));

            RuleFor(x => x.Rate)
                .GreaterThan(0).WithMessage(loc["Val_Rate_MustBePositive"]);

            RuleFor(x => x.Label)
                .MaximumLength(150).WithMessage(loc["Val_MaxLength", 150])
                .When(x => !string.IsNullOrWhiteSpace(x.Label));
        }
    }
}
