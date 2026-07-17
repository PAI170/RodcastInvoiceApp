using RodcastInvoiceApp.Web.Data.Models;

namespace RodcastInvoiceApp.Web.DataTransferObjects.Project
{
    public class ProjectCreateDto
    {
        public int ClientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? CostCenter { get; set; }
        public bool IsActive { get; set; } = true;
        public BillingType BillingType { get; set; }
        public string Config { get; set; } = "{}";
    }
}
