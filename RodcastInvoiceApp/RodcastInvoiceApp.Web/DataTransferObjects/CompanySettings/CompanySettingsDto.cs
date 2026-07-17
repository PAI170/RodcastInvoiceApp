namespace RodcastInvoiceApp.Web.DataTransferObjects.CompanySettings
{
    public class CompanySettingsDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string? LogoPath { get; set; }
        public string? SignaturePath { get; set; }
    }
}
