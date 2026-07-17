using FluentValidation;
using RodcastInvoiceApp.Web.DataTransferObjects.Invoice;

namespace RodcastInvoiceApp.Web.Validators.Invoice
{
    public class InvoiceCreateValidator : AbstractValidator<InvoiceCreateDto>
    {
        public InvoiceCreateValidator()
        {
            RuleFor(x => x.ProjectId)
                .GreaterThan(0).WithMessage("Debes seleccionar un proyecto.");

            RuleFor(x => x.BankAccountId)
                .GreaterThan(0).WithMessage("Debes seleccionar una cuenta bancaria.");

            RuleFor(x => x.InvoiceNumber)
                .NotEmpty().WithMessage("El número de factura es obligatorio.")
                .MaximumLength(50).WithMessage("No puede superar 50 caracteres.");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(x => x.InvoiceDate)
                .WithMessage("La fecha de vencimiento no puede ser anterior a la fecha de la factura.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("La moneda es obligatoria.")
                .Length(3).WithMessage("La moneda debe tener 3 caracteres (ej. USD).");

            RuleFor(x => x.VatPercent)
                .InclusiveBetween(0, 100).WithMessage("El porcentaje de IVA debe estar entre 0 y 100.");

            RuleFor(x => x.VacationDays)
                .GreaterThanOrEqualTo(0).WithMessage("Los días de vacaciones no pueden ser negativos.");

            RuleFor(x => x.WorkedDays)
                .GreaterThanOrEqualTo(0).WithMessage("Los días trabajados no pueden ser negativos.")
                .GreaterThan(0).WithMessage("Si hubo días de vacaciones, indica cuántos días se trabajó.")
                .When(x => x.VacationDays > 0);

            RuleFor(x => x.OvertimeHoursToInvoice)
                .GreaterThanOrEqualTo(0).WithMessage("Las horas extra no pueden ser negativas.");

            RuleFor(x => x.ApprovedAdditionalMinutes)
                .GreaterThanOrEqualTo(0).WithMessage("Los minutos adicionales no pueden ser negativos.");
        }
    }
}
