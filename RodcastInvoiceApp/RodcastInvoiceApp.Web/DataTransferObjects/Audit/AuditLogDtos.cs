namespace RodcastInvoiceApp.Web.DataTransferObjects.Audit
{
    public class InvoiceCreationLogDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class InvoiceEmailLogDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string SentByName { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool SavedToSentFolder { get; set; }
        public bool IsRetry { get; set; }
    }
}
