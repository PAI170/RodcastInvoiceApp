using FluentValidation;
using Microsoft.Extensions.Localization;
using RodcastInvoiceApp.Web.DataTransferObjects.Payment;
using RodcastInvoiceApp.Web.Resources;

namespace RodcastInvoiceApp.Web.Validators.Payment
{
    public class PaymentCreateValidator : AbstractValidator<PaymentCreateDto>
    {
        public PaymentCreateValidator(IStringLocalizer<SharedResource> loc)
        {
            RuleFor(x => x.InvoiceId)
                .GreaterThan(0).WithMessage(loc["Val_Payment_InvoiceRequired"]);

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage(loc["Val_Payment_AmountPositive"]);

            RuleFor(x => x.Method)
                .MaximumLength(50).WithMessage(loc["Val_MaxLength", 50])
                .When(x => !string.IsNullOrWhiteSpace(x.Method));

            RuleFor(x => x.Notes)
                .MaximumLength(250).WithMessage(loc["Val_MaxLength", 250])
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));
        }
    }
}
