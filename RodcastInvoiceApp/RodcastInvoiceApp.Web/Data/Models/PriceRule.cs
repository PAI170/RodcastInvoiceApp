using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RodcastInvoiceApp.Web.Data.Common;

namespace RodcastInvoiceApp.Web.Data.Models
{
    public class PriceRule : BaseEntity
    {
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Dimension1 { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Dimension2 { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Rate { get; set; }

        [StringLength(150)]
        public string? Label { get; set; }
    }
}
