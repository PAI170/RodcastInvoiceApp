using System.ComponentModel.DataAnnotations;
using RodcastInvoiceApp.Web.Data.Common;

namespace RodcastInvoiceApp.Web.Data.Models
{
    public class BankAccount : BaseEntity
    {
        [Required]
        [StringLength(150)]
        public string BankName { get; set; } = string.Empty;

        [StringLength(150)]
        public string AccountHolder { get; set; } = string.Empty;

        [StringLength(50)]
        public string? AccountNumber { get; set; }

        [StringLength(50)]
        public string? Iban { get; set; }

        [StringLength(20)]
        public string? Swift { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
