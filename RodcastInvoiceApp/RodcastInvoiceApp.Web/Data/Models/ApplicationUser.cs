using Microsoft.AspNetCore.Identity;

namespace RodcastInvoiceApp.Web.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;

        // Credenciales SMTP propias de este usuario, para mandar facturas por
        // correo "como" el mismo (cada quien con su propio buzon). La password
        // se guarda encriptada con Data Protection, nunca en texto plano.
        public string? SmtpHost { get; set; }
        public int? SmtpPort { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPasswordProtected { get; set; }

        // Puerto IMAP para guardar copia del correo enviado en "Sent" (mismo
        // host/usuario/contraseña que el SMTP de arriba - mismo buzon, distinto
        // protocolo). Sin esto, mandar por SMTP directo no deja rastro en el
        // cliente de correo (Roundcube) porque SMTP no tiene concepto de carpetas.
        public int? ImapPort { get; set; }

        // Firma HTML propia de este usuario (la misma que usa en su cliente de correo,
        // ej. Outlook). MailKit arma el mensaje a mano y no pasa por ningun cliente de
        // correo, asi que si no se guarda aca el correo sale sin firma.
        public string? EmailSignatureHtml { get; set; }
    }

    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Employee = "Employee";
    }
}
