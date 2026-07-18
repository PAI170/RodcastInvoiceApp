using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.Security;

namespace RodcastInvoiceApp.Web.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITurnstileVerifier _turnstileVerifier;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            ITurnstileVerifier turnstileVerifier,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _turnstileVerifier = turnstileVerifier;
            TurnstileSiteKey = configuration["Turnstile:SiteKey"] ?? string.Empty;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberMe { get; set; } = true;

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public string TurnstileSiteKey { get; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var turnstileToken = Request.Form["cf-turnstile-response"].ToString();
            var isHuman = await _turnstileVerifier.VerifyAsync(turnstileToken, HttpContext.Connection.RemoteIpAddress?.ToString());
            if (!isHuman)
            {
                ErrorMessage = "No se pudo verificar que sos una persona. Intentá de nuevo.";
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                Email, Password, isPersistent: RememberMe, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                // No decimos "cuenta bloqueada" para no confirmarle a quien intenta
                // entrar que el email existe y esta bloqueado especificamente.
                ErrorMessage = result.IsLockedOut
                    ? "Hubo un error, contactá al administrador."
                    : "Email o contraseña incorrectos.";
                return Page();
            }

            return LocalRedirect(string.IsNullOrEmpty(ReturnUrl) ? "/" : ReturnUrl);
        }
    }
}
