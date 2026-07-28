using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.DataTransferObjects.Invoice;

namespace RodcastInvoiceApp.Web.Interfaces
{
    public interface IInvoiceEmailService
    {
        Task<InvoiceEmailPreviewDto> BuildPreviewAsync(int invoiceId, ApplicationUser sender);
        Task ValidateSendableAsync(int invoiceId, ApplicationUser sender);
        Task<InvoiceResponseDto> SendAsync(int invoiceId, ApplicationUser sender);
    }
}
