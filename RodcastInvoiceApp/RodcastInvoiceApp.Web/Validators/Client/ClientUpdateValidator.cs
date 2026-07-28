using FluentValidation;
using Microsoft.Extensions.Localization;
using RodcastInvoiceApp.Web.DataTransferObjects.Client;
using RodcastInvoiceApp.Web.Resources;

namespace RodcastInvoiceApp.Web.Validators.Client
{
    public class ClientUpdateValidator : AbstractValidator<ClientUpdateDto>
    {
        public ClientUpdateValidator(IStringLocalizer<SharedResource> loc)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(loc["Val_Required_ClientName"])
                .MaximumLength(150).WithMessage(loc["Val_MaxLength", 150]);

            RuleFor(x => x.VatId)
                .NotEmpty().WithMessage(loc["Val_Required_VatId"])
                .MaximumLength(50).WithMessage(loc["Val_MaxLength", 50]);

            RuleFor(x => x.Address)
                .MaximumLength(250).WithMessage(loc["Val_MaxLength", 250]);

            RuleFor(x => x.Country)
                .MaximumLength(100).WithMessage(loc["Val_MaxLength", 100]);

            RuleFor(x => x.DefaultCurrency)
                .NotEmpty().WithMessage(loc["Val_Currency_Required"])
                .Length(3).WithMessage(loc["Val_Currency_Length"]);

            RuleFor(x => x.SupplierIdAssigned)
                .MaximumLength(50).WithMessage(loc["Val_MaxLength", 50])
                .When(x => !string.IsNullOrWhiteSpace(x.SupplierIdAssigned));
        }
    }
}
