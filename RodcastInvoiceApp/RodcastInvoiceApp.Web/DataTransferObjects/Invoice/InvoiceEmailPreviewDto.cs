namespace RodcastInvoiceApp.Web.DataTransferObjects.Invoice
{
    public class InvoiceEmailPreviewDto
    {
        public string? ToEmail { get; set; }
        public IReadOnlyList<string> CcEmails { get; set; } = Array.Empty<string>();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? SignatureHtml { get; set; }
        public bool IncludesTimesheet { get; set; }
        public bool SenderConfigured { get; set; }
    }
}
