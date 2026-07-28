using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.DataTransferObjects.Invoice;

namespace RodcastInvoiceApp.Web.Interfaces
{
    public interface IInvoiceEmailService
    {
        Task<InvoiceEmailPreviewDto> BuildPreviewAsync(int invoiceId, ApplicationUser sender);
        Task ValidateSendableAsync(int invoiceId, ApplicationUser sender);
        Task<InvoiceResponseDto> SendAsync(int invoiceId, ApplicationUser sender);

        // Re-archiva en "Sent" via IMAP una factura que ya se mando (accion manual
        // de Admin para cuando el guardado automatico fallo o la factura se mando
        // antes de que existiera esta funcionalidad). No vuelve a mandar el correo.
        Task RetrySentFolderCopyAsync(int invoiceId, ApplicationUser currentUser);
    }
}
