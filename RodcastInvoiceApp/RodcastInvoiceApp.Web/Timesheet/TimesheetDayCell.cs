namespace RodcastInvoiceApp.Web.Timesheet
{
    // Una celda del calendario mensual. Category solo aplica a dias entre
    // semana del mes actual; null ahi significa "Present" (el default).
    // Los fines de semana y los dias de otros meses no se colorean.
    public class TimesheetDayCell
    {
        public int Day { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsWeekend { get; set; }
        public TimesheetDayCategory? Category { get; set; }

        public bool IsColorable => IsCurrentMonth && !IsWeekend;
    }
}
