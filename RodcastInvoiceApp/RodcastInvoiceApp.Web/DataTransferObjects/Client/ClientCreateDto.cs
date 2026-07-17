namespace RodcastInvoiceApp.Web.DataTransferObjects.Client
{
    public class ClientCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string VatId { get; set; } = string.Empty;
        public string? SupplierIdAssigned { get; set; }
        public string DefaultCurrency { get; set; } = "USD";
    }
}
