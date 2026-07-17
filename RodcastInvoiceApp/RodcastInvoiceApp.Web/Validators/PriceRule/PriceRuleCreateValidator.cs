using FluentValidation;
using RodcastInvoiceApp.Web.DataTransferObjects.PriceRule;

namespace RodcastInvoiceApp.Web.Validators.PriceRule
{
    public class PriceRuleCreateValidator : AbstractValidator<PriceRuleCreateDto>
    {
        public PriceRuleCreateValidator()
        {
            RuleFor(x => x.ProjectId)
                .GreaterThan(0).WithMessage("Debes seleccionar un proyecto.");

            RuleFor(x => x.Dimension1)
                .NotEmpty().WithMessage("La primera dimensión (ej. ciudad) es obligatoria.")
                .MaximumLength(100).WithMessage("No puede superar 100 caracteres.");

            RuleFor(x => x.Dimension2)
                .MaximumLength(100).WithMessage("No puede superar 100 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Dimension2));

            RuleFor(x => x.Rate)
                .GreaterThan(0).WithMessage("La tarifa debe ser mayor a 0.");

            RuleFor(x => x.Label)
                .MaximumLength(150).WithMessage("La etiqueta no puede superar 150 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Label));
        }
    }
}
