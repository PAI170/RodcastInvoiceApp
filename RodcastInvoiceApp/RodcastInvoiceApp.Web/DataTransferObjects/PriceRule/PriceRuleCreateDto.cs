namespace RodcastInvoiceApp.Web.DataTransferObjects.PriceRule
{
    public class PriceRuleCreateDto
    {
        public int ProjectId { get; set; }
        public string Dimension1 { get; set; } = string.Empty;
        public string? Dimension2 { get; set; }
        public decimal Rate { get; set; }
        public string? Label { get; set; }
    }
}
