using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.DataTransferObjects.Invoice;
using RodcastInvoiceApp.Web.Exceptions;
using RodcastInvoiceApp.Web.Interfaces;
using RodcastInvoiceApp.Web.Security;

namespace RodcastInvoiceApp.Web.Services
{
    public class InvoiceEmailService : IInvoiceEmailService
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoicePdfService _invoicePdfService;
        private readonly ITimesheetService _timesheetService;
        private readonly IEmailSender _emailSender;
        private readonly ISmtpCredentialsProtector _credentialsProtector;

        public InvoiceEmailService(
            IInvoiceService invoiceService,
            IInvoicePdfService invoicePdfService,
            ITimesheetService timesheetService,
            IEmailSender emailSender,
            ISmtpCredentialsProtector credentialsProtector)
        {
            _invoiceService = invoiceService;
            _invoicePdfService = invoicePdfService;
            _timesheetService = timesheetService;
            _emailSender = emailSender;
            _credentialsProtector = credentialsProtector;
        }

        public async Task<InvoiceEmailPreviewDto> BuildPreviewAsync(int invoiceId, ApplicationUser sender)
        {
            var invoice = await _invoiceService.GetByIdAsync(invoiceId);

            return new InvoiceEmailPreviewDto
            {
                ToEmail = invoice.ClientEmail,
                Subject = BuildSubject(invoice.InvoiceNumber),
                Body = BuildBody(invoice.ClientName, invoice.InvoiceNumber),
                IncludesTimesheet = invoice.HasTimesheet,
                SenderConfigured = IsSenderConfigured(sender)
            };
        }

        public async Task SendAsync(int invoiceId, ApplicationUser sender)
        {
            if (!IsSenderConfigured(sender))
                throw new BadRequestException("Configurá tu correo en \"Mi correo\" antes de poder enviar facturas.");

            var invoice = await _invoiceService.GetByIdAsync(invoiceId);

            if (string.IsNullOrWhiteSpace(invoice.ClientEmail))
                throw new BadRequestException("El cliente no tiene un email configurado.");

            var attachments = new List<EmailAttachment>
            {
                new()
                {
                    FileName = $"Invoice-{invoice.InvoiceNumber}.pdf",
                    Content = await _invoicePdfService.GenerateAsync(invoiceId)
                }
            };

            if (invoice.HasTimesheet)
            {
                attachments.Add(new EmailAttachment
                {
                    FileName = $"Timesheet-{invoice.InvoiceNumber}.pdf",
                    Content = await _timesheetService.GeneratePdfAsync(invoiceId)
                });
            }

            var credentials = new SmtpCredentials
            {
                Host = sender.SmtpHost!,
                Port = sender.SmtpPort!.Value,
                Username = sender.SmtpUsername!,
                Password = _credentialsProtector.Unprotect(sender.SmtpPasswordProtected!),
                FromDisplayName = sender.DisplayName
            };

            await _emailSender.SendAsync(
                credentials, invoice.ClientEmail, BuildSubject(invoice.InvoiceNumber),
                BuildBody(invoice.ClientName, invoice.InvoiceNumber), attachments);
        }

        private static bool IsSenderConfigured(ApplicationUser sender) =>
            !string.IsNullOrWhiteSpace(sender.SmtpHost)
            && sender.SmtpPort is > 0
            && !string.IsNullOrWhiteSpace(sender.SmtpUsername)
            && !string.IsNullOrWhiteSpace(sender.SmtpPasswordProtected);

        private static string BuildSubject(string invoiceNumber) => $"Factura {invoiceNumber} - Rodcast Solutions";

        private static string BuildBody(string clientName, string invoiceNumber) =>
            $"Estimado/a {clientName},\n\n" +
            $"Adjunto la factura {invoiceNumber} y el timesheet correspondiente (si aplica).\n\n" +
            "Saludos,\nRodcast Solutions";
    }
}
