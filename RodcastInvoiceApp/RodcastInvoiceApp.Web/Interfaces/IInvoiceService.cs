using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.DataTransferObjects.Invoice;
using RodcastInvoiceApp.Web.DataTransferObjects.Payment;

namespace RodcastInvoiceApp.Web.Interfaces
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceResponseDto>> GetAllAsync(
            int? projectId = null, int? take = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<InvoiceResponseDto> GetByIdAsync(int id);
        Task<InvoiceResponseDto> CreateAsync(InvoiceCreateDto dto);
        Task<InvoiceResponseDto> UpdateAsync(int id, InvoiceCreateDto dto);
        Task<InvoiceResponseDto> UpdateStatusAsync(int id, InvoiceStatus status);
        Task DeleteAsync(int id);
        Task<PaymentResponseDto> AddPaymentAsync(PaymentCreateDto dto);

        // Para el aviso (no bloqueante) en el formulario: "ya existe una factura
        // para ese mes en este proyecto". excludeInvoiceId se usa al editar, para
        // que la factura no se compare contra si misma.
        Task<bool> HasInvoiceForBillingPeriodAsync(
            int projectId, int billingMonth, int billingYear, int? excludeInvoiceId = null);
    }
}
