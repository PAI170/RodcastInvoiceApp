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
    }

    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Employee = "Employee";
    }
}
