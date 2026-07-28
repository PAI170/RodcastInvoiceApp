namespace RodcastInvoiceApp.Web.DataTransferObjects.Invoice
{
    public class InvoiceEmailApprovalListItemDto
    {
        public int ApprovalId { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string RequestedByName { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
    }

    public class InvoiceEmailApprovalDetailDto
    {
        public int ApprovalId { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string RequestedByName { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public InvoiceEmailPreviewDto Preview { get; set; } = new();
    }
}
