namespace RodcastInvoiceApp.Web.DataTransferObjects.Payment
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string? Method { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
