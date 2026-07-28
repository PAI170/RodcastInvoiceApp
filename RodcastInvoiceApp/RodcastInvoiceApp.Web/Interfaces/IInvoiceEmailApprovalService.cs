using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.DataTransferObjects.Invoice;

namespace RodcastInvoiceApp.Web.Interfaces
{
    public interface IInvoiceEmailApprovalService
    {
        // Punto de entrada unico del boton "Enviar por correo": si quien pide el
        // envio es Admin, manda directo; si no, crea una solicitud pendiente y
        // deja la factura en PendingApproval hasta que un Admin la revise.
        Task<InvoiceResponseDto> RequestSendAsync(int invoiceId, ApplicationUser requestedBy);

        Task<int> GetPendingCountAsync();
        Task<List<InvoiceEmailApprovalListItemDto>> GetPendingAsync();
        Task<InvoiceEmailApprovalDetailDto> GetDetailAsync(int approvalId);
        Task<InvoiceResponseDto> ApproveAsync(int approvalId, ApplicationUser reviewer);
        Task<InvoiceResponseDto> RejectAsync(int approvalId, ApplicationUser reviewer, string comment);
    }
}
