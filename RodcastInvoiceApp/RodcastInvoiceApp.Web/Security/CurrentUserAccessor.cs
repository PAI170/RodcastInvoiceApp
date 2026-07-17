using Microsoft.AspNetCore.Components.Authorization;
using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.Exceptions;

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
    }

    public class CurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public CurrentUserAccessor(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<bool> IsAdminAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User.IsInRole(AppRoles.Admin);
        }

        public async Task EnsureAdminAsync()
        {
            if (!await IsAdminAsync())
                throw new ForbiddenException("No tenés permiso para realizar esta acción.");
        }
    }
}
