using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using RodcastInvoiceApp.Web.Billing;
using RodcastInvoiceApp.Web.Components;
using RodcastInvoiceApp.Web.Data;
using RodcastInvoiceApp.Web.Interfaces;
using RodcastInvoiceApp.Web.Services;
using System.Reflection;

// QuestPDF: licencia Community (gratis para empresas con ingresos anuales menores a 1M USD).
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Base de datos (MariaDB via Pomelo).
// Version fija (no AutoDetect) para que "dotnet ef migrations add" funcione
// sin necesitar una conexion real. Ajustar al version real de tu MariaDB en CloudPanel.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 6, 0))));

// FluentValidation: registra todos los validadores del proyecto (busca clases AbstractValidator<T>).
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Mapster: escanea las clases IRegister (ej. MappingConfig) para las reglas de mapeo.
var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
typeAdapterConfig.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(typeAdapterConfig);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// Services
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
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
