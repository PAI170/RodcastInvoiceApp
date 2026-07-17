using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.DataTransferObjects.Payment;

namespace RodcastInvoiceApp.Web.DataTransferObjects.Invoice
{
    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ProjectCostCenter { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientAddress { get; set; } = string.Empty;
        public string ClientVatId { get; set; } = string.Empty;
        public string? ClientSupplierIdAssigned { get; set; }
        public int BankAccountId { get; set; }
        public string BankAccountName { get; set; } = string.Empty;
        public string? BankAccountIban { get; set; }
        public string? BankAccountSwift { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal VatPercent { get; set; }
        public bool IsVatExonerated { get; set; }
        public InvoiceStatus Status { get; set; }
        public string? PaymentMethod { get; set; }

        public string? TicketNumber { get; set; }
        public string? City { get; set; }
        public string? SlaType { get; set; }
        public int? AdditionalMinutes { get; set; }

        public int VacationDays { get; set; }
        public int WorkedDays { get; set; }
        public decimal OvertimeHoursToInvoice { get; set; }

        // True si ya se guardo un timesheet para esta factura al menos una vez
        // (aunque sea sin ninguna excepcion, todo "Present").
        public bool HasTimesheet { get; set; }

        public List<InvoiceItemResponseDto> Items { get; set; } = new();
        public List<PaymentResponseDto> Payments { get; set; } = new();

        // Calculados a partir de Items y Payments (no son columnas propias).
        public decimal Subtotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal Total { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Misma regla que valida InvoiceService: solo se puede editar/eliminar
        // una factura en borrador y sin pagos registrados.
        public bool CanEditOrDelete => Status == InvoiceStatus.Draft && Payments.Count == 0;
    }
}
