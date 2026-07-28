using FluentValidation;
using Microsoft.Extensions.Localization;
using RodcastInvoiceApp.Web.DataTransferObjects.Invoice;
using RodcastInvoiceApp.Web.Resources;

namespace RodcastInvoiceApp.Web.Validators.Invoice
{
    public class InvoiceCreateValidator : AbstractValidator<InvoiceCreateDto>
    {
        public InvoiceCreateValidator(IStringLocalizer<SharedResource> loc)
        {
            RuleFor(x => x.ProjectId)
                .GreaterThan(0).WithMessage(loc["Val_Select_Project"]);

            RuleFor(x => x.BankAccountId)
                .GreaterThan(0).WithMessage(loc["Val_Select_BankAccount"]);

            RuleFor(x => x.InvoiceNumber)
                .NotEmpty().WithMessage(loc["Val_Required_InvoiceNumber"])
                .MaximumLength(50).WithMessage(loc["Val_MaxLength", 50]);

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(x => x.InvoiceDate)
                .WithMessage(loc["Val_DueDate_BeforeInvoiceDate"]);

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage(loc["Val_Currency_Required"])
                .Length(3).WithMessage(loc["Val_Currency_Length"]);

            RuleFor(x => x.VatPercent)
                .InclusiveBetween(0, 100).WithMessage(loc["Val_VatPercent_Range"]);

            RuleFor(x => x.VacationDays)
                .GreaterThanOrEqualTo(0).WithMessage(loc["Val_VacationDays_Negative"]);

            RuleFor(x => x.WorkedDays)
                .GreaterThanOrEqualTo(0).WithMessage(loc["Val_WorkedDays_Negative"])
                .GreaterThan(0).WithMessage(loc["Val_WorkedDays_RequiredWithVacation"])
                .When(x => x.VacationDays > 0);

            RuleFor(x => x.OvertimeHoursToInvoice)
                .GreaterThanOrEqualTo(0).WithMessage(loc["Val_OvertimeHours_Negative"]);

            RuleFor(x => x.ApprovedAdditionalMinutes)
                .GreaterThanOrEqualTo(0).WithMessage(loc["Val_AdditionalMinutes_Negative"]);
        }
    }
}
