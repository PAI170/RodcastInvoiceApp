using FluentValidation;
using RodcastInvoiceApp.Web.DataTransferObjects.BankAccount;

namespace RodcastInvoiceApp.Web.Validators.BankAccount
{
    public class BankAccountUpdateValidator : AbstractValidator<BankAccountUpdateDto>
    {
        public BankAccountUpdateValidator()
        {
            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("El nombre del banco es obligatorio.")
                .MaximumLength(150).WithMessage("No puede superar 150 caracteres.");

            RuleFor(x => x.AccountHolder)
                .MaximumLength(150).WithMessage("No puede superar 150 caracteres.");

            RuleFor(x => x.AccountNumber)
                .MaximumLength(50).WithMessage("No puede superar 50 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.AccountNumber));

            RuleFor(x => x.Iban)
                .MaximumLength(50).WithMessage("No puede superar 50 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Iban));

            RuleFor(x => x.Swift)
                .MaximumLength(20).WithMessage("No puede superar 20 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Swift));

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("La moneda es obligatoria.")
                .Length(3).WithMessage("La moneda debe tener 3 caracteres (ej. USD).");
        }
    }
}
