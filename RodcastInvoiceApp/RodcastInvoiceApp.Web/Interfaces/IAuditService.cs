using RodcastInvoiceApp.Web.DataTransferObjects.Audit;

namespace RodcastInvoiceApp.Web.Interfaces
{
    public interface IAuditService
    {
        Task<List<InvoiceCreationLogDto>> GetInvoiceCreationLogAsync();
        Task<List<InvoiceEmailLogDto>> GetEmailSendLogAsync();
    }
}
