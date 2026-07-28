using FluentValidation;
using Microsoft.Extensions.Localization;
using RodcastInvoiceApp.Web.DataTransferObjects.BankAccount;
using RodcastInvoiceApp.Web.Resources;

namespace RodcastInvoiceApp.Web.Validators.BankAccount
{
    public class BankAccountCreateValidator : AbstractValidator<BankAccountCreateDto>
    {
        public BankAccountCreateValidator(IStringLocalizer<SharedResource> loc)
        {
            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage(loc["Val_Required_BankName"])
                .MaximumLength(150).WithMessage(loc["Val_MaxLength", 150]);

            RuleFor(x => x.AccountHolder)
                .MaximumLength(150).WithMessage(loc["Val_MaxLength", 150]);

            RuleFor(x => x.AccountNumber)
                .MaximumLength(50).WithMessage(loc["Val_MaxLength", 50])
                .When(x => !string.IsNullOrWhiteSpace(x.AccountNumber));

            RuleFor(x => x.Iban)
                .MaximumLength(50).WithMessage(loc["Val_MaxLength", 50])
                .When(x => !string.IsNullOrWhiteSpace(x.Iban));

            RuleFor(x => x.Swift)
                .MaximumLength(20).WithMessage(loc["Val_MaxLength", 20])
                .When(x => !string.IsNullOrWhiteSpace(x.Swift));

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage(loc["Val_Currency_Required"])
                .Length(3).WithMessage(loc["Val_Currency_Length"]);
        }
    }
}
