using System.ComponentModel.DataAnnotations;
using RodcastInvoiceApp.Web.Data.Common;

namespace RodcastInvoiceApp.Web.Data.Models
{
    public class Client : BaseEntity
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string VatId { get; set; } = string.Empty;

        // El ID que el propio cliente le asigna a Rodcast como proveedor (ej. "001351" para HEM).
        // No es un dato nuestro, vive en el sistema del cliente que lo asignó.
        [StringLength(50)]
        public string? SupplierIdAssigned { get; set; }

        [Required]
        [StringLength(3)]
        public string DefaultCurrency { get; set; } = "USD";

        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
