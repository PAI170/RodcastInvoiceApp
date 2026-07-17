using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RodcastInvoiceApp.Web.Data.Common;

namespace RodcastInvoiceApp.Web.Data.Models
{
    public enum InvoiceStatus
    {
        Draft,
        Sent,
        Paid,
        Overdue
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

        [Column(TypeName = "decimal(10,2)")]
        public decimal OvertimeHoursToInvoice { get; set; }

        // JSON con la lista de dias que no son "Present" (ver Timesheet.TimesheetDayException).
        // Null = todavia no se ha generado/guardado ningun timesheet para esta factura.
        public string? TimesheetExceptions { get; set; }

        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
