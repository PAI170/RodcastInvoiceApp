namespace RodcastInvoiceApp.Web.DataTransferObjects.Payment
{
    public class PaymentCreateDto
    {
        public int InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Today;
        public decimal Amount { get; set; }
        public string? Method { get; set; }
        public string? Notes { get; set; }
    }
}
