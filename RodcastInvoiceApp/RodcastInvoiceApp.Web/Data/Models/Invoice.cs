using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Localization;
using RodcastInvoiceApp.Web.Data.Common;
using RodcastInvoiceApp.Web.Resources;

namespace RodcastInvoiceApp.Web.Data.Models
{
    public enum InvoiceStatus
    {
        Draft,
        Sent,
        Paid,
        // Reemplaza al viejo "Overdue": el correo con la factura quedo pedido
        // pero todavia no lo aprobo el admin (ver InvoiceEmailApproval).
        PendingApproval
    }

    public static class InvoiceStatusExtensions
    {
        // Recibe el localizador en vez de tener el texto fijo: asi el estado se
        // traduce con el resto de la UI en vez de quedar siempre en un idioma.
        public static string ToDisplayText(this InvoiceStatus status, IStringLocalizer<SharedResource> loc) => status switch
        {
            InvoiceStatus.Draft => loc["Status_Draft"],
            InvoiceStatus.Sent => loc["Status_Sent"],
            InvoiceStatus.Paid => loc["Status_Paid"],
            InvoiceStatus.PendingApproval => loc["Status_PendingApproval"],
            _ => status.ToString()
        };
    }

    public class Invoice : BaseEntity
    {
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public int BankAccountId { get; set; }
        public virtual BankAccount BankAccount { get; set; } = null!;

        // Texto libre: preserva la numeracion historica (001-012) y permite
        // continuar la secuencia manualmente (013, 014, ...).
        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        [Column(TypeName = "decimal(5,2)")]
        public decimal VatPercent { get; set; } = 13m;

        // Bandera visual/reporte; el calculo real siempre usa VatPercent (se pone en 0 cuando aplica).
        public bool IsVatExonerated { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        // Solo aplican a proyectos per_ticket.
        [StringLength(50)]
        public string? TicketNumber { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? SlaType { get; set; }

        public int? AdditionalMinutes { get; set; }

        // Solo aplican a proyectos monthly_retainer. Se guardan (aunque el
        // calculo ya quedo hecho en los InvoiceItems) para poder editar la
        // factura despues sin perder los datos originales del mes.
        public int VacationDays { get; set; }
        public int WorkedDays { get; set; }

        // Mes/anio que se factura (el retainer se cobra por adelantado: el 28 de
        // julio se factura agosto). Independiente de InvoiceDate, que es la fecha
        // real en que se emite la factura. Solo aplica a monthly_retainer.
        public int BillingMonth { get; set; }
        public int BillingYear { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal OvertimeHoursToInvoice { get; set; }

        // JSON con la lista de dias que no son "Present" (ver Timesheet.TimesheetDayException).
        // Null = todavia no se ha generado/guardado ningun timesheet para esta factura.
        public string? TimesheetExceptions { get; set; }

        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<InvoiceEmailApproval> EmailApprovals { get; set; } = new List<InvoiceEmailApproval>();
    }
}
