namespace RodcastInvoiceApp.Web.Billing
{
    // Una linea de factura calculada, antes de guardarse como InvoiceItem.
    public class InvoiceItemDraft
    {
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal Rate { get; set; }

        // Monto realmente cobrado (puede diferir de Quantity x Rate por redondeo, ver InvoiceItem.cs).
        public decimal Amount { get; set; }
    }
}
