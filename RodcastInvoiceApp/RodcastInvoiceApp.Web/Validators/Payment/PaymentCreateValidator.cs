using FluentValidation;
using RodcastInvoiceApp.Web.DataTransferObjects.Payment;

namespace RodcastInvoiceApp.Web.Validators.Payment
{
    public class PaymentCreateValidator : AbstractValidator<PaymentCreateDto>
    {
        public PaymentCreateValidator()
        {
            RuleFor(x => x.InvoiceId)
                .GreaterThan(0).WithMessage("Falta indicar la factura.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto del pago debe ser mayor a 0.");

            RuleFor(x => x.Method)
                .MaximumLength(50).WithMessage("No puede superar 50 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Method));

            RuleFor(x => x.Notes)
                .MaximumLength(250).WithMessage("No puede superar 250 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));
        }
    }
}
