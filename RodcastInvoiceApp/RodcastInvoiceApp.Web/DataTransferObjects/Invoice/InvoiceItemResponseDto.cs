namespace RodcastInvoiceApp.Web.DataTransferObjects.Invoice
{
    public class InvoiceItemResponseDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
}
