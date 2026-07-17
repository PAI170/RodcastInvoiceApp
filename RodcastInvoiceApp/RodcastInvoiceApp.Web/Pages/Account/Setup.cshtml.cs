using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RodcastInvoiceApp.Web.Data.Models;

namespace RodcastInvoiceApp.Web.Pages.Account
{
    // Publica a proposito: es la unica forma de crear el primer Admin sin acceso previo.
    // Se autobloquea en cuanto existe un usuario (ver OnGetAsync/OnPostAsync).
    [AllowAnonymous]
    public class SetupModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SetupModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public string DisplayName { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public bool AlreadyInitialized { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            AlreadyInitialized = await _userManager.Users.AnyAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (await _userManager.Users.AnyAsync())
            {
                AlreadyInitialized = true;
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Email,
                Email = Email,
                DisplayName = DisplayName
            };

            var result = await _userManager.CreateAsync(user, Password);
            if (!result.Succeeded)
            {
                ErrorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                return Page();
            }

            await _userManager.AddToRoleAsync(user, AppRoles.Admin);
            return Redirect("/account/login");
        }
    }
}
