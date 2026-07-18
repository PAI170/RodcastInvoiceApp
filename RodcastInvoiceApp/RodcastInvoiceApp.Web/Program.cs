using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
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

// Login/Logout usan Razor Pages clasicas: necesitan escribir la cookie de
// autenticacion en una respuesta HTTP normal, algo que no se puede hacer
// dentro de un circuito interactivo de Blazor Server (SignalR).
builder.Services.AddRazorPages();

// Cloudflare Turnstile: valida el captcha del login contra la API de Cloudflare.
builder.Services.AddHttpClient<ITurnstileVerifier, TurnstileVerifier>();

// Base de datos (MariaDB via Pomelo).
// Version fija (no AutoDetect) para que "dotnet ef migrations add" funcione
// sin necesitar una conexion real. Ajustar al version real de tu MariaDB en CloudPanel.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 6, 0))));

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

// Todas las paginas requieren estar autenticado por defecto; las que sean
// publicas (login) se marcan con [AllowAnonymous] explicitamente.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddCascadingAuthenticationState();

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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// CSS/JS/imagenes deben cargar siempre, incluso en la pagina de login
// (que no es Blazor y no tiene por que estar detras del FallbackPolicy).
app.MapStaticAssets().AllowAnonymous();
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

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

app.MapGet("/invoices/{id:int}/timesheet-pdf", async (int id, HttpContext httpContext, ITimesheetService timesheetService, bool download = false) =>
{
    var timesheet = await timesheetService.GetAsync(id);
    var bytes = await timesheetService.GeneratePdfAsync(id);
    var disposition = download ? "attachment" : "inline";
    httpContext.Response.Headers.ContentDisposition = $"{disposition}; filename=\"Timesheet-{timesheet.Year}-{timesheet.Month:00}.pdf\"";
    return Results.File(bytes, "application/pdf");
});

app.Run();
