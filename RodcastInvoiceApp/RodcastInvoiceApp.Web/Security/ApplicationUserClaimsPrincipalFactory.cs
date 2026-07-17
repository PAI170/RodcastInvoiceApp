using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RodcastInvoiceApp.Web.Data.Models;

namespace RodcastInvoiceApp.Web.Security
{
    // Agrega el nombre (DisplayName) como claim para poder mostrarlo en la UI
    // sin tener que consultar la base de datos en cada render del NavMenu.
    //
    // OJO: hereda de la variante <TUser, TRole> (no <TUser>) a proposito -
    // es la que agrega el claim de rol (Admin/Employee). Usar la variante sin
    // TRole pisa el factory por defecto y deja a todos sin rol.
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public ApplicationUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);
            ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim("DisplayName", user.DisplayName));
            return principal;
        }
    }
}
