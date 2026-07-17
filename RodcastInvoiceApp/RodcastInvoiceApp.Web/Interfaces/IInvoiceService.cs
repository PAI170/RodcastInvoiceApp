using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.DataTransferObjects.Invoice;
using RodcastInvoiceApp.Web.DataTransferObjects.Payment;

namespace RodcastInvoiceApp.Web.Interfaces
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceResponseDto>> GetAllAsync(int? projectId = null, int? take = null);
        Task<InvoiceResponseDto> GetByIdAsync(int id);
        Task<InvoiceResponseDto> CreateAsync(InvoiceCreateDto dto);
        Task<InvoiceResponseDto> UpdateAsync(int id, InvoiceCreateDto dto);
        Task<InvoiceResponseDto> UpdateStatusAsync(int id, InvoiceStatus status);
        Task DeleteAsync(int id);
        Task<PaymentResponseDto> AddPaymentAsync(PaymentCreateDto dto);
    }
}
