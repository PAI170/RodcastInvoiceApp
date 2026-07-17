using RodcastInvoiceApp.Web.DataTransferObjects.Timesheet;

namespace RodcastInvoiceApp.Web.Interfaces
{
    public interface ITimesheetService
    {
        Task<TimesheetDto> GetAsync(int invoiceId);
        Task<TimesheetDto> SaveAsync(int invoiceId, TimesheetSaveDto dto);
        Task<byte[]> GeneratePdfAsync(int invoiceId);
    }
}
