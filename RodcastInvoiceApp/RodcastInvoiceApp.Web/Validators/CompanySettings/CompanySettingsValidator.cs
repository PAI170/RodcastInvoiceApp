using FluentValidation;
using RodcastInvoiceApp.Web.DataTransferObjects.CompanySettings;

namespace RodcastInvoiceApp.Web.Validators.CompanySettings
{
    public class CompanySettingsValidator : AbstractValidator<CompanySettingsDto>
    {
        public CompanySettingsValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la empresa es obligatorio.")
                .MaximumLength(150).WithMessage("No puede superar 150 caracteres.");

            RuleFor(x => x.TaxId)
                .NotEmpty().WithMessage("El Tax ID es obligatorio.")
                .MaximumLength(50).WithMessage("No puede superar 50 caracteres.");

            RuleFor(x => x.Address)
                .MaximumLength(250).WithMessage("No puede superar 250 caracteres.");
        }
    }
}
