using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using RodcastInvoiceApp.Web.Data;
using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.DataTransferObjects.Timesheet;
using RodcastInvoiceApp.Web.Exceptions;
using RodcastInvoiceApp.Web.Interfaces;
using RodcastInvoiceApp.Web.Pdf;
using RodcastInvoiceApp.Web.Timesheet;

namespace RodcastInvoiceApp.Web.Services
{
    public class TimesheetService : ITimesheetService
    {
        private readonly AppDbContext _context;
        private readonly ICompanySettingsService _companySettingsService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TimesheetService(
            AppDbContext context,
            ICompanySettingsService companySettingsService,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _companySettingsService = companySettingsService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<TimesheetDto> GetAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Project)
                .FirstOrDefaultAsync(i => i.Id == invoiceId)
                ?? throw new NotFoundException("Factura no encontrada.");

            return BuildDto(invoice);
        }

        public async Task<TimesheetDto> SaveAsync(int invoiceId, TimesheetSaveDto dto)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Project)
                .FirstOrDefaultAsync(i => i.Id == invoiceId)
                ?? throw new NotFoundException("Factura no encontrada.");

            var daysInMonth = DateTime.DaysInMonth(invoice.InvoiceDate.Year, invoice.InvoiceDate.Month);

            foreach (var exception in dto.Exceptions)
            {
                if (exception.Day < 1 || exception.Day > daysInMonth)
                    throw new BadRequestException($"El día {exception.Day} no es válido para este mes.");

                var date = new DateTime(invoice.InvoiceDate.Year, invoice.InvoiceDate.Month, exception.Day);
                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    throw new BadRequestException($"El día {exception.Day} es fin de semana, no se puede marcar.");
            }

            invoice.TimesheetExceptions = JsonSerializer.Serialize(dto.Exceptions);
            await _context.SaveChangesAsync();

            return BuildDto(invoice);
        }

        public async Task<byte[]> GeneratePdfAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Project)
                .FirstOrDefaultAsync(i => i.Id == invoiceId)
                ?? throw new NotFoundException("Factura no encontrada.");

            var dto = BuildDto(invoice);
            var company = await _companySettingsService.GetAsync();
            var logoBytes = WebRootFileReader.TryRead(_webHostEnvironment, company.LogoPath);

            var document = new TimesheetPdfDocument(dto, logoBytes);
            return document.GeneratePdf();
        }

        private static TimesheetDto BuildDto(Invoice invoice)
        {
            var exceptions = ParseExceptions(invoice.TimesheetExceptions);
            var weeks = TimesheetCalendarBuilder.BuildWeeks(
                invoice.InvoiceDate.Year, invoice.InvoiceDate.Month, exceptions);

            return new TimesheetDto
            {
                InvoiceId = invoice.Id,
                Month = invoice.InvoiceDate.Month,
                Year = invoice.InvoiceDate.Year,
                ProjectName = invoice.Project.Name,
                Weeks = weeks
            };
        }

        private static List<TimesheetDayException> ParseExceptions(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<TimesheetDayException>();

            return JsonSerializer.Deserialize<List<TimesheetDayException>>(json)
                ?? new List<TimesheetDayException>();
        }
    }
}
