using Microsoft.Extensions.Localization;
using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.DataTransferObjects.Invoice;
using RodcastInvoiceApp.Web.Exceptions;
using RodcastInvoiceApp.Web.Interfaces;
using RodcastInvoiceApp.Web.Resources;
using RodcastInvoiceApp.Web.Security;

namespace RodcastInvoiceApp.Web.Services
{
    public class InvoiceEmailService : IInvoiceEmailService
    {
        // ======================= TEMPORAL - MODO PRUEBAS =======================
        // Mientras se prueba como llegan los correos, se pisa el destinatario real
        // y se vacia el CC. Sacar esto (volver a "invoice.ClientEmail" en los dos
        // usos de abajo, y restaurar el CC real) cuando el usuario avise que ya
        // termino de probar.
        private const string TestOnlyRecipientOverride = "davidrp97@outlook.com";

        // Fijo: siempre van en copia, nadie los puede editar desde la UI.
        // CC real (restaurar al terminar las pruebas):
        //   "fernando.catala@hemmersbach.com", "maciej.wrobel@hemmersbach.com"
        private static readonly string[] FixedCcRecipients = Array.Empty<string>();
        // ==================== FIN TEMPORAL - MODO PRUEBAS ======================

        private readonly IInvoiceService _invoiceService;
        private readonly IInvoicePdfService _invoicePdfService;
        private readonly ITimesheetService _timesheetService;
        private readonly IEmailSender _emailSender;
        private readonly ISmtpCredentialsProtector _credentialsProtector;
        private readonly IStringLocalizer<SharedResource> _loc;

        public InvoiceEmailService(
            IInvoiceService invoiceService,
            IInvoicePdfService invoicePdfService,
            ITimesheetService timesheetService,
            IEmailSender emailSender,
            ISmtpCredentialsProtector credentialsProtector,
            IStringLocalizer<SharedResource> loc)
        {
            _invoiceService = invoiceService;
            _invoicePdfService = invoicePdfService;
            _timesheetService = timesheetService;
            _emailSender = emailSender;
            _credentialsProtector = credentialsProtector;
            _loc = loc;
        }

        public async Task<InvoiceEmailPreviewDto> BuildPreviewAsync(int invoiceId, ApplicationUser sender)
        {
            var invoice = await _invoiceService.GetByIdAsync(invoiceId);

            return new InvoiceEmailPreviewDto
            {
                ToEmail = TestOnlyRecipientOverride, // TEMPORAL - PRUEBAS (ver arriba)
                CcEmails = FixedCcRecipients,
                Subject = BuildSubject(invoice.InvoiceNumber),
                Body = BuildBody(invoice.InvoiceNumber),
                SignatureHtml = sender.EmailSignatureHtml,
                IncludesTimesheet = invoice.HasTimesheet,
                SenderConfigured = IsSenderConfigured(sender)
            };
        }

        public async Task ValidateSendableAsync(int invoiceId, ApplicationUser sender)
        {
            if (!IsSenderConfigured(sender))
                throw new BadRequestException(_loc["SvcErr_EmailSenderNotConfigured"]);

            var invoice = await _invoiceService.GetByIdAsync(invoiceId);

            if (string.IsNullOrWhiteSpace(invoice.ClientEmail))
                throw new BadRequestException(_loc["SvcErr_ClientNoEmail"]);

            if (!invoice.HasTimesheet)
                throw new BadRequestException(_loc["SvcErr_TimesheetNotGenerated"]);
        }

        public async Task<InvoiceResponseDto> SendAsync(int invoiceId, ApplicationUser sender)
        {
            await ValidateSendableAsync(invoiceId, sender);

            var invoice = await _invoiceService.GetByIdAsync(invoiceId);

            var attachments = new List<EmailAttachment>
            {
                new()
                {
                    FileName = $"Invoice-{invoice.InvoiceNumber}.pdf",
                    Content = await _invoicePdfService.GenerateAsync(invoiceId)
                },
                new()
                {
                    FileName = $"Timesheet-{invoice.InvoiceNumber}.pdf",
                    Content = await _timesheetService.GeneratePdfAsync(invoiceId)
                }
            };

            var credentials = new SmtpCredentials
            {
                Host = sender.SmtpHost!,
                Port = sender.SmtpPort!.Value,
                Username = sender.SmtpUsername!,
                Password = _credentialsProtector.Unprotect(sender.SmtpPasswordProtected!),
                FromDisplayName = sender.DisplayName
            };

            await _emailSender.SendAsync(
                credentials, TestOnlyRecipientOverride, FixedCcRecipients, BuildSubject(invoice.InvoiceNumber), // TEMPORAL - PRUEBAS (ver arriba)
                BuildBody(invoice.InvoiceNumber), sender.EmailSignatureHtml, attachments);

            return await _invoiceService.UpdateStatusAsync(invoiceId, InvoiceStatus.Sent);
        }

        private static bool IsSenderConfigured(ApplicationUser sender) =>
            !string.IsNullOrWhiteSpace(sender.SmtpHost)
            && sender.SmtpPort is > 0
            && !string.IsNullOrWhiteSpace(sender.SmtpUsername)
            && !string.IsNullOrWhiteSpace(sender.SmtpPasswordProtected);

        private static string BuildSubject(string invoiceNumber) => $"Invoice {invoiceNumber}";

        private static string BuildBody(string invoiceNumber) => $"Invoice {invoiceNumber} attached.";
    }
}
