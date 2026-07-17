namespace RodcastInvoiceApp.Web.Interfaces
{
    public interface IInvoicePdfService
    {
        Task<byte[]> GenerateAsync(int invoiceId);
    }
}
