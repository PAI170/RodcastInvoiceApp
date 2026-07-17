namespace RodcastInvoiceApp.Web.Billing
{
    public abstract class BillingInput
    {
    }

    // Datos de captura mensual para proyectos monthly_retainer.
    public class MonthlyRetainerInput : BillingInput
    {
        public int Month { get; set; }
        public int Year { get; set; }

        // 0 = mes normal (dispara el monto fijo). > 0 = mes con vacaciones (se factura por horas).
        public int VacationDays { get; set; }

        // Solo se usa cuando VacationDays > 0.
        public int WorkedDays { get; set; }

        // Horas extra del mes anterior a facturar en esta factura, en una linea aparte.
        public decimal OvertimeHoursToInvoice { get; set; }
    }

    // Datos de un ticket individual para proyectos per_ticket (1 ticket = 1 factura).
    public class PerTicketInput : BillingInput
    {
        public string TicketNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string SlaType { get; set; } = string.Empty;

        // Minutos adicionales solo si el SDM los aprobo; si no hay aprobacion, se deja en 0
        // y simplemente no se factura (decision ya tomada: no se guarda como no facturable).
        public int ApprovedAdditionalMinutes { get; set; }

        // Horas extra del ticket (poco probable, mismo esquema 1.5x que monthly_retainer).
        public decimal OvertimeHours { get; set; }
    }
}
