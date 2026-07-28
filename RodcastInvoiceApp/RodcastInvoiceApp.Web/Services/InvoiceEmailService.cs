using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using RodcastInvoiceApp.Web.Data;
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
        // Fijo: siempre van en copia, nadie los puede editar desde la UI.
        private static readonly string[] FixedCcRecipients =
        {
            "fernando.catala@hemmersbach.com",
            "maciej.wrobel@hemmersbach.com"
        };

        private readonly AppDbContext _context;
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoicePdfService _invoicePdfService;
        private readonly ITimesheetService _timesheetService;
        private readonly IEmailSender _emailSender;
        private readonly ISmtpCredentialsProtector _credentialsProtector;
        private readonly ICurrentUserAccessor _currentUser;
        private readonly IStringLocalizer<SharedResource> _loc;

        public InvoiceEmailService(
            AppDbContext context,
            IInvoiceService invoiceService,
            IInvoicePdfService invoicePdfService,
            ITimesheetService timesheetService,
            IEmailSender emailSender,
            ISmtpCredentialsProtector credentialsProtector,
            ICurrentUserAccessor currentUser,
            IStringLocalizer<SharedResource> loc)
        {
            _context = context;
            _invoiceService = invoiceService;
            _invoicePdfService = invoicePdfService;
            _timesheetService = timesheetService;
            _emailSender = emailSender;
            _credentialsProtector = credentialsProtector;
            _currentUser = currentUser;
            _loc = loc;
        }

        public async Task<InvoiceEmailPreviewDto> BuildPreviewAsync(int invoiceId, ApplicationUser sender)
        {
            var invoice = await _invoiceService.GetByIdAsync(invoiceId);

            return new InvoiceEmailPreviewDto
            {
                ToEmail = invoice.ClientEmail,
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
                ImapPort = sender.ImapPort,
                Username = sender.SmtpUsername!,
                Password = _credentialsProtector.Unprotect(sender.SmtpPasswordProtected!),
                FromDisplayName = sender.DisplayName
            };

            var savedToSent = await _emailSender.SendAsync(
                credentials, invoice.ClientEmail!, FixedCcRecipients, BuildSubject(invoice.InvoiceNumber),
                BuildBody(invoice.InvoiceNumber), sender.EmailSignatureHtml, attachments);

            var result = await _invoiceService.UpdateStatusAsync(invoiceId, InvoiceStatus.Sent);

            // Se guarda aparte del resto de la factura: es lo unico que necesita
            // "Reintentar guardado en Sent" para saber que buzon usar despues.
            var invoiceEntity = await _context.Invoices.FirstAsync(i => i.Id == invoiceId);
            invoiceEntity.SentByUserId = sender.Id;

            _context.InvoiceEmailLogs.Add(new InvoiceEmailLog
            {
                InvoiceId = invoiceId,
                SentByUserId = sender.Id,
                ToEmail = invoice.ClientEmail!,
                SavedToSentFolder = savedToSent,
                IsRetry = false
            });

            await _context.SaveChangesAsync();

            return result;
        }

        public async Task RetrySentFolderCopyAsync(int invoiceId, ApplicationUser currentUser)
        {
            await _currentUser.EnsureAdminAsync();

            var invoice = await _invoiceService.GetByIdAsync(invoiceId);
            if (invoice.Status is not (InvoiceStatus.Sent or InvoiceStatus.Paid))
                throw new BadRequestException(_loc["SvcErr_InvoiceNotSentYet"]);

            var sender = await ResolveOriginalSenderAsync(invoiceId) ?? currentUser;

            if (!IsSenderConfigured(sender))
                throw new BadRequestException(_loc["SvcErr_EmailSenderNotConfigured"]);
            if (sender.ImapPort is not > 0)
                throw new BadRequestException(_loc["SvcErr_ImapNotConfigured"]);

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
                ImapPort = sender.ImapPort,
                Username = sender.SmtpUsername!,
                Password = _credentialsProtector.Unprotect(sender.SmtpPasswordProtected!),
                FromDisplayName = sender.DisplayName
            };

            await _emailSender.SaveCopyToSentAsync(
                credentials, invoice.ClientEmail!, FixedCcRecipients, BuildSubject(invoice.InvoiceNumber),
                BuildBody(invoice.InvoiceNumber), sender.EmailSignatureHtml, attachments);

            // Si SaveCopyToSentAsync tiro excepcion no llegamos aca - no se loguea
            // un reintento fallido, el admin ya se entera por el mensaje de error.
            _context.InvoiceEmailLogs.Add(new InvoiceEmailLog
            {
                InvoiceId = invoiceId,
                SentByUserId = sender.Id,
                ToEmail = invoice.ClientEmail!,
                SavedToSentFolder = true,
                IsRetry = true
            });
            await _context.SaveChangesAsync();
        }

        // Prioridad: 1) quien realmente la mando (guardado en SendAsync), 2) para
        // facturas mandadas antes de que existiera SentByUserId, la ultima solicitud
        // de aprobacion ya aprobada (con esas credenciales se mando en su momento).
        private async Task<ApplicationUser?> ResolveOriginalSenderAsync(int invoiceId)
        {
            var invoiceEntity = await _context.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoiceEntity?.SentByUserId is not null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == invoiceEntity.SentByUserId);
                if (user is not null)
                    return user;
            }

            var approval = await _context.InvoiceEmailApprovals
                .AsNoTracking()
                .Include(a => a.RequestedByUser)
                .Where(a => a.InvoiceId == invoiceId && a.Status == EmailApprovalStatus.Approved)
                .OrderByDescending(a => a.ReviewedAt)
                .FirstOrDefaultAsync();

            return approval?.RequestedByUser;
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
