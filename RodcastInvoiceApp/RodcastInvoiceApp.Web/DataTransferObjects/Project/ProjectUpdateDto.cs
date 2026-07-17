using RodcastInvoiceApp.Web.Data.Models;

namespace RodcastInvoiceApp.Web.DataTransferObjects.Project
{
    // No incluye ClientId: un proyecto no cambia de cliente una vez creado.
    public class ProjectUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? CostCenter { get; set; }
        public bool IsActive { get; set; } = true;
        public BillingType BillingType { get; set; }
        public string Config { get; set; } = "{}";
    }
}
