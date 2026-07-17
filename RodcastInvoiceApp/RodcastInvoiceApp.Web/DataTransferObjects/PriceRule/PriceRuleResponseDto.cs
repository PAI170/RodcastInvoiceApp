namespace RodcastInvoiceApp.Web.DataTransferObjects.PriceRule
{
    public class PriceRuleResponseDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Dimension1 { get; set; } = string.Empty;
        public string? Dimension2 { get; set; }
        public decimal Rate { get; set; }
        public string? Label { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
