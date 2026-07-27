using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace RodcastInvoiceApp.Web.Security
{
    // El FallbackPolicy exige login por defecto en cualquier endpoint sin
    // [Authorize]/[AllowAnonymous] explicito. Los assets bajo /_framework
    // (blazor.web.js, el hub de SignalR, etc.) caen en esa categoria segun
    // como los registre el SDK version a version - en vez de adivinar cual
    // Map... es responsable, este requirement los deja pasar siempre por
    // el prefijo de la ruta, sin importar el endpoint que los sirva.
    public class FrameworkAssetOrAuthenticatedRequirement : IAuthorizationRequirement
    {
    }

    public class FrameworkAssetOrAuthenticatedHandler : AuthorizationHandler<FrameworkAssetOrAuthenticatedRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FrameworkAssetOrAuthenticatedHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, FrameworkAssetOrAuthenticatedRequirement requirement)
        {
            var path = _httpContextAccessor.HttpContext?.Request.Path ?? PathString.Empty;

            if (path.StartsWithSegments("/_framework") || context.User.Identity?.IsAuthenticated == true)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
