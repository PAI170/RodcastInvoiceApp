using RodcastInvoiceApp.Web.DataTransferObjects.CompanySettings;

namespace RodcastInvoiceApp.Web.Interfaces
{
    public interface ICompanySettingsService
    {
        Task<CompanySettingsDto> GetAsync();
        Task<CompanySettingsDto> UpdateAsync(CompanySettingsDto dto);
    }
}
