using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RodcastInvoiceApp.Web.Data.Common;

namespace RodcastInvoiceApp.Web.Data.Models
{
    public class Payment : BaseEntity
    {
        public int InvoiceId { get; set; }
        public virtual Invoice Invoice { get; set; } = null!;

        public DateTime PaymentDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string? Method { get; set; }

        [StringLength(250)]
        public string? Notes { get; set; }
    }
}
