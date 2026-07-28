namespace RodcastInvoiceApp.Web.DataTransferObjects.Invoice
{
    // Un solo DTO para ambos tipos de cobro (igual que Invoice en la base de datos):
    // los campos que no aplican al tipo de proyecto simplemente se dejan en su valor por defecto.
    public class InvoiceCreateDto
    {
        public int ProjectId { get; set; }
        public int BankAccountId { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; } = DateTime.Today;
        public DateTime DueDate { get; set; } = DateTime.Today;
        public string Currency { get; set; } = "USD";
        public decimal VatPercent { get; set; } = 13m;
        public bool IsVatExonerated { get; set; }
        public string? PaymentMethod { get; set; }

        // Solo aplican a proyectos monthly_retainer.
        public int VacationDays { get; set; }
        public int WorkedDays { get; set; }
        public decimal OvertimeHoursToInvoice { get; set; }

        // Mes/anio que se factura (independiente de InvoiceDate - el retainer se
        // cobra por adelantado). 0 = todavia no elegido.
        public int BillingMonth { get; set; }
        public int BillingYear { get; set; } = DateTime.Today.Year;

        // Solo aplican a proyectos per_ticket.
        public string? TicketNumber { get; set; }
        public string? City { get; set; }
        public string? SlaType { get; set; }
        public int ApprovedAdditionalMinutes { get; set; }
    }
}
