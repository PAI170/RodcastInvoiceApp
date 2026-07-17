using RodcastInvoiceApp.Web.Timesheet;

namespace RodcastInvoiceApp.Web.DataTransferObjects.Timesheet
{
    public class TimesheetDto
    {
        public int InvoiceId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public List<List<TimesheetDayCell>> Weeks { get; set; } = new();
    }
}
