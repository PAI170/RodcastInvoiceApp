using QuestPDF.Fluent;
using RodcastInvoiceApp.Web.Interfaces;
using RodcastInvoiceApp.Web.Pdf;

namespace RodcastInvoiceApp.Web.Services
{
    public class InvoicePdfService : IInvoicePdfService
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ICompanySettingsService _companySettingsService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InvoicePdfService(
            IInvoiceService invoiceService,
            ICompanySettingsService companySettingsService,
            IWebHostEnvironment webHostEnvironment)
        {
            _invoiceService = invoiceService;
            _companySettingsService = companySettingsService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<byte[]> GenerateAsync(int invoiceId)
        {
            var invoice = await _invoiceService.GetByIdAsync(invoiceId);
            var company = await _companySettingsService.GetAsync();

            var logoBytes = WebRootFileReader.TryRead(_webHostEnvironment, company.LogoPath);
            var signatureBytes = WebRootFileReader.TryRead(_webHostEnvironment, company.SignaturePath);

            var document = new InvoicePdfDocument(invoice, company, logoBytes, signatureBytes);
            return document.GeneratePdf();
        }
    }
}
