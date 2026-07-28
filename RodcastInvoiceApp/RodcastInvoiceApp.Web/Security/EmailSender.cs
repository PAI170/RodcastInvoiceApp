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
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromDisplayName { get; set; } = string.Empty;
    }

    public interface IEmailSender
    {
        Task SendAsync(
            SmtpCredentials credentials, string toEmail, IEnumerable<string> ccEmails, string subject,
            string body, string? signatureHtml, IEnumerable<EmailAttachment> attachments);
    }

    public class MailKitEmailSender : IEmailSender
    {
        public async Task SendAsync(
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

            using var client = new SmtpClient();
            // Auto detecta el modo correcto segun el puerto (465 = SSL directo, 587 = STARTTLS).
            await client.ConnectAsync(credentials.Host, credentials.Port, SecureSocketOptions.Auto);
            await client.AuthenticateAsync(credentials.Username, credentials.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
