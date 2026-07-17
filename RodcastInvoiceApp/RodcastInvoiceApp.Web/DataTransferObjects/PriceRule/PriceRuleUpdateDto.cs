namespace RodcastInvoiceApp.Web.DataTransferObjects.PriceRule
{
    // No incluye ProjectId: una tarifa no cambia de proyecto una vez creada.
    public class PriceRuleUpdateDto
    {
        public string Dimension1 { get; set; } = string.Empty;
        public string? Dimension2 { get; set; }
        public decimal Rate { get; set; }
        public string? Label { get; set; }
    }
}
