using Mapster;
using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.DataTransferObjects.BankAccount;
using RodcastInvoiceApp.Web.DataTransferObjects.Client;
using RodcastInvoiceApp.Web.DataTransferObjects.Invoice;
using RodcastInvoiceApp.Web.DataTransferObjects.Payment;
using RodcastInvoiceApp.Web.DataTransferObjects.PriceRule;
using RodcastInvoiceApp.Web.DataTransferObjects.Project;

namespace RodcastInvoiceApp.Web.Mappings
{
    public class MappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ClientCreateDto, Client>();
            config.NewConfig<ClientUpdateDto, Client>();

            config.NewConfig<Client, ClientResponseDto>()
                .Map(dest => dest.ProjectCount, src => src.Projects.Count);

            config.NewConfig<ProjectCreateDto, Project>();
            config.NewConfig<ProjectUpdateDto, Project>();

            config.NewConfig<Project, ProjectResponseDto>()
                .Map(dest => dest.ClientName, src => src.Client.Name)
                .Map(dest => dest.PriceRuleCount, src => src.PriceRules.Count);

            config.NewConfig<PriceRuleCreateDto, PriceRule>();
            config.NewConfig<PriceRuleUpdateDto, PriceRule>();
            config.NewConfig<PriceRule, PriceRuleResponseDto>();

            config.NewConfig<BankAccountCreateDto, BankAccount>();
            config.NewConfig<BankAccountUpdateDto, BankAccount>();
            config.NewConfig<BankAccount, BankAccountResponseDto>();

            config.NewConfig<DataTransferObjects.CompanySettings.CompanySettingsDto, CompanySettings>();
            config.NewConfig<CompanySettings, DataTransferObjects.CompanySettings.CompanySettingsDto>();

            config.NewConfig<InvoiceItem, InvoiceItemResponseDto>();
            config.NewConfig<PaymentCreateDto, Payment>();
            config.NewConfig<Payment, PaymentResponseDto>();
        }
    }
}
