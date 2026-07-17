using RodcastInvoiceApp.Web.Timesheet;

namespace RodcastInvoiceApp.Web.DataTransferObjects.Timesheet
{
    public class TimesheetSaveDto
    {
        public List<TimesheetDayException> Exceptions { get; set; } = new();
    }
}
