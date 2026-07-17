using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RodcastInvoiceApp.Web.Data.Common;

namespace RodcastInvoiceApp.Web.Data.Models
{
    public class InvoiceItem : BaseEntity
    {
        public int InvoiceId { get; set; }
        public virtual Invoice Invoice { get; set; } = null!;

        [Required]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;

        // Quantity y Rate son informativos (se imprimen en el detalle de la factura).
        // El monto realmente cobrado es Amount, para evitar diferencias de centavos
        // por redondeo (ej. el mes normal del retainer: 173.18h x $10.22 no da
        // exactamente $1769.91, pero Amount si se fija en ese valor exacto).
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }

        [StringLength(20)]
        public string Unit { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
    }
}
