using Microsoft.AspNetCore.DataProtection;

namespace RodcastInvoiceApp.Web.Security
{
    // Encripta/desencripta la contraseña SMTP de cada usuario antes de guardarla
    // en ApplicationUser.SmtpPasswordProtected. Nunca se guarda en texto plano.
    public interface ISmtpCredentialsProtector
    {
        string Protect(string plainPassword);
        string Unprotect(string protectedPassword);
    }

    public class SmtpCredentialsProtector : ISmtpCredentialsProtector
    {
        private readonly IDataProtector _protector;

        public SmtpCredentialsProtector(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("RodcastInvoiceApp.SmtpCredentials.v1");
        }

        public string Protect(string plainPassword) => _protector.Protect(plainPassword);

        public string Unprotect(string protectedPassword) => _protector.Unprotect(protectedPassword);
    }
}
