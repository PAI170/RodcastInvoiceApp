using System.ComponentModel.DataAnnotations;
using RodcastInvoiceApp.Web.Data.Common;

namespace RodcastInvoiceApp.Web.Data.Models
{
    public enum EmailApprovalStatus
    {
        Pending,
        Approved,
        Rejected
    }

    // Cada vez que un usuario no-admin pide mandar una factura por correo se crea
    // una fila de estas (Pending). El admin la aprueba (dispara el envio real, con
    // las credenciales del que la pidio) o la rechaza (la factura vuelve a Borrador
    // y el motivo queda guardado aca para que el remitente sepa que corregir).
    public class InvoiceEmailApproval : BaseEntity
    {
        public int InvoiceId { get; set; }
        public virtual Invoice Invoice { get; set; } = null!;

        public string RequestedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser RequestedByUser { get; set; } = null!;

        public EmailApprovalStatus Status { get; set; } = EmailApprovalStatus.Pending;

        public string? ReviewedByUserId { get; set; }
        public virtual ApplicationUser? ReviewedByUser { get; set; }
        public DateTime? ReviewedAt { get; set; }

        [StringLength(1000)]
        public string? RejectionComment { get; set; }
    }
}
