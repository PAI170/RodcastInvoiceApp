using FluentValidation;
using RodcastInvoiceApp.Web.DataTransferObjects.Client;

namespace RodcastInvoiceApp.Web.Validators.Client
{
    public class ClientUpdateValidator : AbstractValidator<ClientUpdateDto>
    {
        public ClientUpdateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del cliente es obligatorio.")
                .MaximumLength(150).WithMessage("El nombre no puede superar 150 caracteres.");

            RuleFor(x => x.VatId)
                .NotEmpty().WithMessage("El VAT ID es obligatorio.")
                .MaximumLength(50).WithMessage("El VAT ID no puede superar 50 caracteres.");

            RuleFor(x => x.Address)
                .MaximumLength(250).WithMessage("La dirección no puede superar 250 caracteres.");

            RuleFor(x => x.Country)
                .MaximumLength(100).WithMessage("El país no puede superar 100 caracteres.");

            RuleFor(x => x.DefaultCurrency)
                .NotEmpty().WithMessage("La moneda es obligatoria.")
                .Length(3).WithMessage("La moneda debe tener 3 caracteres (ej. USD).");

            RuleFor(x => x.SupplierIdAssigned)
                .MaximumLength(50).WithMessage("El ID de proveedor no puede superar 50 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.SupplierIdAssigned));
        }
    }
}
