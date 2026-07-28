using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace RodcastInvoiceApp.Web.Security
{
    public class EmailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/pdf";
    }

    public class SmtpCredentials
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public int? ImapPort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromDisplayName { get; set; } = string.Empty;
    }

    public interface IEmailSender
    {
        // Devuelve si la copia en "Sent" se guardo bien (para el log de auditoria).
        // El correo se manda igual por SMTP sin importar este resultado.
        Task<bool> SendAsync(
            SmtpCredentials credentials, string toEmail, IEnumerable<string> ccEmails, string subject,
            string body, string? signatureHtml, IEnumerable<EmailAttachment> attachments);

        // Guarda (o re-guarda) una copia en "Sent" via IMAP sin mandar nada por SMTP.
        // A diferencia del guardado best-effort de SendAsync, esto SI deja que la
        // excepcion se propague: lo llama una accion manual del admin que necesita
        // saber si funciono o no, no un envio automatico que no se debe interrumpir.
        Task SaveCopyToSentAsync(
            SmtpCredentials credentials, string toEmail, IEnumerable<string> ccEmails, string subject,
            string body, string? signatureHtml, IEnumerable<EmailAttachment> attachments);
    }

    public class MailKitEmailSender : IEmailSender
    {
        public async Task<bool> SendAsync(
            SmtpCredentials credentials, string toEmail, IEnumerable<string> ccEmails, string subject,
            string body, string? signatureHtml, IEnumerable<EmailAttachment> attachments)
        {
            var message = BuildMessage(credentials, toEmail, ccEmails, subject, body, signatureHtml, attachments);

            using (var smtpClient = new SmtpClient())
            {
                // Auto detecta el modo correcto segun el puerto (465 = SSL directo, 587 = STARTTLS).
                await smtpClient.ConnectAsync(credentials.Host, credentials.Port, SecureSocketOptions.Auto);
                await smtpClient.AuthenticateAsync(credentials.Username, credentials.Password);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);
            }

            // SMTP no tiene concepto de "carpeta Enviados" (eso es IMAP): un cliente
            // de correo como Roundcube la llena solo cuando el envio pasa por su propio
            // compositor, algo que no ocurre aca porque mandamos directo por SMTP. Por
            // eso, despues de un envio exitoso, guardamos nosotros mismos una copia en
            // "Sent" via IMAP APPEND - best effort: si esto falla (imap mal configurado,
            // servidor caido) el correo YA salio, asi que no rompemos el envio por esto.
            if (credentials.ImapPort is > 0)
            {
                try
                {
                    await AppendToSentAsync(credentials, message);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public Task SaveCopyToSentAsync(
            SmtpCredentials credentials, string toEmail, IEnumerable<string> ccEmails, string subject,
            string body, string? signatureHtml, IEnumerable<EmailAttachment> attachments)
        {
            var message = BuildMessage(credentials, toEmail, ccEmails, subject, body, signatureHtml, attachments);
            return AppendToSentAsync(credentials, message);
        }

        private static MimeMessage BuildMessage(
            SmtpCredentials credentials, string toEmail, IEnumerable<string> ccEmails, string subject,
            string body, string? signatureHtml, IEnumerable<EmailAttachment> attachments)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(credentials.FromDisplayName, credentials.Username));
            message.To.Add(MailboxAddress.Parse(toEmail));
            foreach (var cc in ccEmails)
                message.Cc.Add(MailboxAddress.Parse(cc));
            message.Subject = subject;

            // MailKit no pasa por ningun cliente de correo (Outlook, Gmail, etc.), asi que
            // la firma no se agrega sola: EmailHtmlRenderer arma el HtmlBody con el mensaje
            // mas la firma. Misma funcion que usa la pantalla de aprobaciones para la vista
            // previa, para que el admin vea exactamente lo que se va a mandar.
            var bodyBuilder = new BodyBuilder { TextBody = body };
            if (!string.IsNullOrWhiteSpace(signatureHtml))
                bodyBuilder.HtmlBody = EmailHtmlRenderer.BuildHtmlBody(body, signatureHtml);

            foreach (var attachment in attachments)
            {
                bodyBuilder.Attachments.Add(
                    attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
            }

            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        private static async Task AppendToSentAsync(SmtpCredentials credentials, MimeMessage message)
        {
            if (credentials.ImapPort is not > 0)
                throw new InvalidOperationException("IMAP port not configured for this sender.");

            using var imapClient = new ImapClient();
            await imapClient.ConnectAsync(credentials.Host, credentials.ImapPort!.Value, SecureSocketOptions.Auto);
            await imapClient.AuthenticateAsync(credentials.Username, credentials.Password);

            // SPECIAL-USE (RFC 6154) deja pedirle al servidor "la carpeta que vos uses
            // para Enviados", sin adivinar el nombre exacto (varia: "Sent", "Sent Items",
            // "INBOX.Sent" segun el servidor). Dovecot (lo mas comun en hosting tipo
            // CloudPanel) la soporta de fabrica.
            var sentFolder = imapClient.GetFolder(SpecialFolder.Sent);
            if (sentFolder is not null)
            {
                await sentFolder.OpenAsync(FolderAccess.ReadWrite);
                await sentFolder.AppendAsync(message, MessageFlags.Seen);
                await sentFolder.CloseAsync();
            }

            await imapClient.DisconnectAsync(true);
        }
    }
}
