namespace RodcastInvoiceApp.Web.Timesheet
{
    // Forma en la que se guarda cada excepcion dentro del JSON de Invoice.TimesheetExceptions.
    public class TimesheetDayException
    {
        public int Day { get; set; }
        public TimesheetDayCategory Category { get; set; }
    }
}
