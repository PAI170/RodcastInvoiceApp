using System.Text.Json;
using FluentValidation;
using RodcastInvoiceApp.Web.DataTransferObjects.Project;

namespace RodcastInvoiceApp.Web.Validators.Project
{
    public class ProjectCreateValidator : AbstractValidator<ProjectCreateDto>
    {
        public ProjectCreateValidator()
        {
            RuleFor(x => x.ClientId)
                .GreaterThan(0).WithMessage("Debes seleccionar un cliente.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del proyecto es obligatorio.")
                .MaximumLength(150).WithMessage("El nombre no puede superar 150 caracteres.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("El código del proyecto es obligatorio.")
                .MaximumLength(50).WithMessage("El código no puede superar 50 caracteres.");

            RuleFor(x => x.CostCenter)
                .MaximumLength(50).WithMessage("El centro de costo no puede superar 50 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.CostCenter));

            RuleFor(x => x.BillingType)
                .IsInEnum().WithMessage("El tipo de cobro no es válido.");

            RuleFor(x => x.Config)
                .Must(BeValidJson).WithMessage("La configuración debe ser un JSON válido.");
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
