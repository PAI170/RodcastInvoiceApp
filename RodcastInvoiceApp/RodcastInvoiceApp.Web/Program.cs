using System.Globalization;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using RodcastInvoiceApp.Web.Billing;
using RodcastInvoiceApp.Web.Components;
using RodcastInvoiceApp.Web.Data;
using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.Interfaces;
using RodcastInvoiceApp.Web.Security;
using RodcastInvoiceApp.Web.Services;
using System.Reflection;

// QuestPDF: licencia Community (gratis para empresas con ingresos anuales menores a 1M USD).
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Idiomas: es/en globales para los TEXTOS de la UI (UICulture). La Culture que
// controla formato de numeros/fechas queda fija en en-US siempre, sin importar
// el idioma elegido - asi los montos en pantalla y en el PDF de facturas/timesheets
// nunca cambian de formato (1234.56, nunca 1234,56) al tocar el switch ES/ENG.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en");
    options.SupportedCultures = new[] { new CultureInfo("en-US") };
    options.SupportedUICultures = new[] { new CultureInfo("en"), new CultureInfo("es") };
    // El default ya incluye un CookieRequestCultureProvider que lee la cookie
    // ".AspNetCore.Culture" - /set-culture (mas abajo) es lo unico que la escribe.
});

// Login/Logout usan Razor Pages clasicas: necesitan escribir la cookie de
// autenticacion en una respuesta HTTP normal, algo que no se puede hacer
// dentro de un circuito interactivo de Blazor Server (SignalR).
builder.Services.AddRazorPages();

// Cloudflare Turnstile: valida el captcha del login contra la API de Cloudflare.
builder.Services.AddHttpClient<ITurnstileVerifier, TurnstileVerifier>();

// Base de datos (MariaDB via Pomelo).
// Version fija (no AutoDetect) para que "dotnet ef migrations add" funcione
// sin necesitar una conexion real. Ajustar al version real de tu MariaDB en CloudPanel.
// AddDbContextFactory (no AddDbContext a secas): sigue registrando AppDbContext
// como scoped de siempre para los Services existentes, pero ADEMAS deja pedir
// IDbContextFactory<AppDbContext> donde haga falta un contexto propio,
// independiente del compartido por el circuito. Se necesita en NavMenu.razor
// (la campanita de aprobaciones consulta la base en paralelo con lo que sea que
// la pagina actual tambien este consultando - dos operaciones concurrentes sobre
// el mismo DbContext tiran "A second operation was started on this context...").
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 6, 0))));

// Data Protection: sin persistir la clave, un reinicio del contenedor invalida
// las cookies de sesion Y las contraseñas SMTP encriptadas por usuario (pagina
// "Ajustes"). "keys" debe montarse como volumen en Docker para sobrevivir.
var dataProtectionPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
        .SetApplicationName("RodcastInvoiceApp");
}

// ASP.NET Core Identity: cookie de autenticacion + roles (Admin / Employee).
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        // Reglas de password relajadas: app interna, 2 usuarios conocidos.
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;

        // Bloqueo automatico por fuerza bruta: 3 intentos fallidos y se bloquea
        // (mismo bloqueo que ya usa Users.razor, se desbloquea solo a los 5 min
        // o antes a mano desde /users).
        options.Lockout.MaxFailedAccessAttempts = 3;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

// Revalida la cookie contra el SecurityStamp del usuario cada minuto: si un Admin
// bloquea a alguien (Users.razor llama UpdateSecurityStampAsync), la sesion activa
// de esa persona se corta sola en <= 1 minuto, no solo se le impide volver a entrar.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(1);
});

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.LoginPath = "/account/login";
        options.AccessDeniedPath = "/account/login";
        options.Events.OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAuthorizationHandler, FrameworkAssetOrAuthenticatedHandler>();

// Todas las paginas requieren estar autenticado por defecto; las que sean
// publicas (login) se marcan con [AllowAnonymous] explicitamente. Los
// assets bajo /_framework (blazor.web.js, el hub de SignalR) siempre pasan,
// via FrameworkAssetOrAuthenticatedRequirement - ver Security/FrameworkAssetAuthorization.cs
// para el por que (no dependemos de adivinar que Map... los registra).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .AddRequirements(new FrameworkAssetOrAuthenticatedRequirement())
        .Build();
});

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddSingleton<ISmtpCredentialsProtector, SmtpCredentialsProtector>();
builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();

// FluentValidation: registra todos los validadores del proyecto (busca clases AbstractValidator<T>).
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Mapster: escanea las clases IRegister (ej. MappingConfig) para las reglas de mapeo.
var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
typeAdapterConfig.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(typeAdapterConfig);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// Services
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IPriceRuleService, PriceRuleService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<ICompanySettingsService, CompanySettingsService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IInvoicePdfService, InvoicePdfService>();
builder.Services.AddScoped<ITimesheetService, TimesheetService>();
builder.Services.AddScoped<IInvoiceEmailService, InvoiceEmailService>();
builder.Services.AddScoped<IInvoiceEmailApprovalService, InvoiceEmailApprovalService>();

// Billing strategies: cada una se registra por separado y se resuelven todas
// como IEnumerable<IBillingStrategy> en InvoiceService.
builder.Services.AddScoped<IBillingStrategy, MonthlyRetainerBillingStrategy>();
builder.Services.AddScoped<IBillingStrategy, PerTicketBillingStrategy>();

var app = builder.Build();

// Crea los roles Admin/Employee si no existen todavia (idempotente, no crea usuarios).
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { AppRoles.Admin, AppRoles.Employee })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// Siembra las firmas HTML fijas una sola vez (solo si el usuario ya existe y
// todavia no tiene una firma guardada, para no pisar lo que cargue despues a mano).
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    async Task SeedSignatureAsync(string email, string signatureHtml)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null && string.IsNullOrWhiteSpace(user.EmailSignatureHtml))
        {
            user.EmailSignatureHtml = signatureHtml;
            await userManager.UpdateAsync(user);
        }
    }

    await SeedSignatureAsync(
        RodcastInvoiceApp.Web.Data.Seed.EmailSignatureSeedData.DavidRodriguezEmail,
        RodcastInvoiceApp.Web.Data.Seed.EmailSignatureSeedData.DavidRodriguezHtml);
    await SeedSignatureAsync(
        RodcastInvoiceApp.Web.Data.Seed.EmailSignatureSeedData.DanielaCastroEmail,
        RodcastInvoiceApp.Web.Data.Seed.EmailSignatureSeedData.DanielaCastroHtml);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// CSS/JS/imagenes deben cargar siempre, incluso en la pagina de login
// (que no es Blazor y no tiene por que estar detras del FallbackPolicy).
app.MapStaticAssets().AllowAnonymous();
app.MapRazorPages();

// AllowAnonymous acá es sobre el endpoint HTTP (deja pasar siempre el script
// _framework/blazor.web.js y la conexion del circuito, sin los cuales la app
// no carga ni para redirigir a login). La proteccion real de cada pagina
// sigue intacta: la hace AuthorizeRouteView (Routes.razor) dentro del propio
// arbol de componentes, evaluando el usuario autenticado independientemente
// de esta politica HTTP.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

// Endpoints para ver/descargar el PDF de una factura/timesheet (mas
// confiable que JS interop para archivos binarios en Blazor Server).
// Sin "?download=true": Content-Disposition "inline", para mostrarlo dentro
// de un <iframe> (vista previa en un modal).
// Con "?download=true": Content-Disposition "attachment", para forzar la
// descarga directa (mismo PDF, solo cambia este encabezado).
app.MapGet("/invoices/{id:int}/pdf", async (int id, HttpContext httpContext, IInvoiceService invoiceService, IInvoicePdfService pdfService, bool download = false) =>
{
    var invoice = await invoiceService.GetByIdAsync(id);
    var bytes = await pdfService.GenerateAsync(id);
    var disposition = download ? "attachment" : "inline";
    httpContext.Response.Headers.ContentDisposition = $"{disposition}; filename=\"Invoice-{invoice.InvoiceNumber}.pdf\"";
    return Results.File(bytes, "application/pdf");
});

// Blazor Server no puede cambiar el idioma "en caliente" dentro del circuito
// activo: hay que recargar la pagina para que el nuevo request (y el circuito
// nuevo que arranca con el) tome la cookie de cultura actualizada. El boton del
// navbar apunta aca con un <a href> normal (no NavigateTo), asi el browser hace
// una recarga real en vez de una navegacion dentro del circuito.
app.MapGet("/set-culture", (string uiCulture, string redirectUri, HttpContext httpContext) =>
{
    httpContext.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture: "en-US", uiCulture: uiCulture)),
        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

    return Results.LocalRedirect(redirectUri);
});

app.MapGet("/invoices/{id:int}/timesheet-pdf", async (int id, HttpContext httpContext, ITimesheetService timesheetService, bool download = false) =>
{
    var timesheet = await timesheetService.GetAsync(id);
    var bytes = await timesheetService.GeneratePdfAsync(id);
    var disposition = download ? "attachment" : "inline";
    httpContext.Response.Headers.ContentDisposition = $"{disposition}; filename=\"Timesheet-{timesheet.Year}-{timesheet.Month:00}.pdf\"";
    return Results.File(bytes, "application/pdf");
});

app.Run();
