using System.ComponentModel.DataAnnotations;
using RodcastInvoiceApp.Web.Data.Common;

namespace RodcastInvoiceApp.Web.Data.Models
{
    // Tabla de una sola fila: los datos fijos de Rodcast para generar el PDF de la factura.
    public class CompanySettings : BaseEntity
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TaxId { get; set; } = string.Empty;

        [StringLength(255)]
        public string? LogoPath { get; set; }

        [StringLength(255)]
        public string? SignaturePath { get; set; }
    }
}
