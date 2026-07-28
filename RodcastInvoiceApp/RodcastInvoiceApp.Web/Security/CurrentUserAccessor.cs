using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.Exceptions;
using RodcastInvoiceApp.Web.Resources;

namespace RodcastInvoiceApp.Web.Security
{
    // Segunda capa de permisos, independiente de la UI: los Services la usan
    // para no depender solo de que un boton este oculto en una pagina de Blazor.
    //
    // Usa AuthenticationStateProvider (no IHttpContextAccessor): en Blazor Server,
    // HttpContext solo es confiable durante el request HTTP inicial, no durante
    // eventos del circuito (clicks) que es cuando estos Services se ejecutan.
    public interface ICurrentUserAccessor
    {
        Task<bool> IsAdminAsync();
        Task EnsureAdminAsync();
        Task<string?> GetUserIdAsync();
    }

    public class CurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IStringLocalizer<SharedResource> _loc;

        public CurrentUserAccessor(
            AuthenticationStateProvider authenticationStateProvider, IStringLocalizer<SharedResource> loc)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _loc = loc;
        }

        public async Task<bool> IsAdminAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User.IsInRole(AppRoles.Admin);
        }

        public async Task EnsureAdminAsync()
        {
            if (!await IsAdminAsync())
                throw new ForbiddenException(_loc["SvcErr_Forbidden"]);
        }

        public async Task<string?> GetUserIdAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
