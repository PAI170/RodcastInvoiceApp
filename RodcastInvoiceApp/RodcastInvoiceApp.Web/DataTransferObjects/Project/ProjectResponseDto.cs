using RodcastInvoiceApp.Web.Data.Models;

namespace RodcastInvoiceApp.Web.DataTransferObjects.Project
{
    public class ProjectResponseDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? CostCenter { get; set; }
        public bool IsActive { get; set; }
        public BillingType BillingType { get; set; }
        public string Config { get; set; } = string.Empty;
        public int PriceRuleCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
