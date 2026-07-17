# Estado del Proyecto — RodcastInvoiceApp

Este documento resume qué se construyó, cómo está organizado, y qué falta. Para el diseño de negocio original (reglas de facturación, contrato con HEM, etc.) ver [`resumen-sistema-facturacion.md`](resumen-sistema-facturacion.md) — ese sigue siendo la fuente de verdad del "por qué"; este documento es el "qué se hizo" y "qué falta".

## Stack y ubicación

- **Solución**: `D:\Dev\RodcastInvoiceApp\RodcastInvoiceApp\` (el archivo de solución quedó como `RodcastInvoiceApp..slnx`, con doble punto — es cosmético, no afecta nada).
- **Proyecto**: `RodcastInvoiceApp.Web` — Blazor Server, **.NET 10** (LTS), interactividad global.
- **Base de datos**: MariaDB en el servidor casero (CloudPanel), IP `192.168.0.248:3306`, base `RodcastInvoicesDB`, usuario `RodcastAdmin`. La conexión real vive en **user-secrets** de Visual Studio, no en `appsettings.json` (ese solo tiene un placeholder).
- **Paquetes clave**: `Pomelo.EntityFrameworkCore.MySql` **9.0.0** (fijo — Pomelo todavía no soporta EF Core 10), `Microsoft.EntityFrameworkCore.Tools` **9.0.x** (no subir a 10.x), `Mapster`, `FluentValidation`, `QuestPDF` (licencia Community).

## Arquitectura (patrón establecido, mantenerlo)

- **Sin Controllers/API REST**: los componentes Blazor inyectan los `Services` directamente (`IClientService`, `IInvoiceService`, etc.). No hay capa HTTP intermedia porque no hay frontend separado.
- **Capas**: `Data/Models` (entidades EF Core) → `DataTransferObjects/{Entidad}` (Create/Update/Response DTOs) → `Validators/{Entidad}` (FluentValidation) → `Services` (lógica de negocio, valida con FluentValidation manualmente ya que no hay Controllers que lo hagan automático) → páginas en `Components/Pages`.
- **Excepciones tipadas** (`Exceptions/AppExceptions.cs`): `NotFoundException`, `ConflictException`, `BadRequestException` — las páginas Blazor las atrapan con `catch (Exception ex) when (ex is ... or ...)` y muestran el mensaje en un `<div class="alert alert-danger">`.
- **BaseEntity simple**: `Id`, `CreatedAt`, `UpdatedAt` — sin soft-delete ni auditoría (se dejó así a propósito, ver `resumen-sistema-facturacion.md`).
- **Enums en la BD**: se guardan como texto (`HasConversion<string>().HasMaxLength(n)`), no como número.
- **Mapster**: `.Adapt<T>()` para objetos ya materializados, `.ProjectToType<T>()` dentro de queries `IQueryable` (nunca mezclar un método C# compilado dentro de un `.Select()` de EF Core — no se traduce a SQL).
- **JSON para datos flexibles**: `Project.Config` (parámetros de facturación por tipo de cobro) e `Invoice.TimesheetExceptions` (excepciones del calendario) son columnas `json` en MariaDB.

## Completado

### Fase 1 — Clientes
CRUD completo (`Client`, DTOs, validador, `ClientService`, página `/clients`). Patrón base replicado en todo lo demás.

### Fase 2 — Proyectos y Tarifas
- `Project` completo: `Code` (único por cliente), `CostCenter`, `IsActive`, `BillingType` (`MonthlyRetainer`/`PerTicket`), `Config` (JSON).
- `PriceRule` (tarifas por ciudad/SLA para proyectos `per_ticket`), en cascada con el proyecto.
- Páginas `/projects` y `/projects/{id}/price-rules`.

### Fase 3 — Facturas
- `Billing/IBillingStrategy` + `MonthlyRetainerBillingStrategy` (mes normal = monto fijo del contrato; mes con vacaciones = por horas; hora extra 1.5x) + `PerTicketBillingStrategy` (tarifa base por ciudad/SLA + tiempo adicional en incrementos de 15 min).
- `InvoiceItem.Amount` separado de `Quantity × Rate` (evita diferencias de centavos por redondeo).
- `InvoiceService`: crear, **editar** (recalcula los items con la estrategia correcta), eliminar, pagos, cambio de estado.
- **Reglas de estado ya implementadas**: solo se puede editar/eliminar una factura en estado `Draft` y sin pagos registrados (`InvoiceResponseDto.CanEditOrDelete`); una vez `Paid`, el estado queda congelado (no se puede volver a cambiar).
- Páginas: `/invoices` (listado + filtro por proyecto), `/invoices/{id}` (detalle + pagos), `/invoices/new` y `/invoices/{id}/edit` (mismo componente `NewInvoice.razor` para ambos modos).
- `BankAccount` y `CompanySettings` (fila única) con sus páginas.

### Fase 4 — PDF de factura
- QuestPDF, `Pdf/InvoicePdfDocument.cs`, replica la plantilla histórica exacta (barra azul, logo, dos columnas de datos, tabla de items, totales, firma con línea, cuenta bancaria).
- Logo y firma de Rodcast guardados en `wwwroot/images/`, rutas configuradas en `/company-settings`.

### Extra — Timesheet mensual
- `Timesheet/` (namespace): `TimesheetDayCategory` (enum), `TimesheetCalendarBuilder` (arma la cuadrícula de semanas), `TimesheetColors` (colores/etiquetas compartidos entre la UI y el PDF).
- `Invoice.TimesheetExceptions` (JSON, solo guarda los días que NO son "Present").
- Página `/invoices/{id}/timesheet` (calendario clickeable), `Pdf/TimesheetPdfDocument.cs` (replica el diseño oscuro con calendario coloreado + leyenda).
- `InvoiceResponseDto.HasTimesheet`: los botones "Ver/Descargar Timesheet" solo aparecen si ya se guardó uno.

### Vista previa de PDFs
- Endpoints `/invoices/{id}/pdf` y `/invoices/{id}/timesheet-pdf` sirven el PDF con `Content-Disposition: inline` por defecto (para verlo en un `<iframe>`), o `attachment` con `?download=true` (para forzar descarga).
- `Components/Shared/PdfPreviewModal.razor`: modal reutilizable (sin JS, solo estado de Blazor) usado en `InvoiceDetail.razor` e `InvoiceTimesheet.razor` para "Ver Factura"/"Ver Timesheet".

### UI general
- `Home.razor` ("Sistema de Facturación"): clientes recientes (últimos 5) y últimas facturas (últimas 10) — ambos con `take` a nivel de query para no traer toda la tabla.
- `NavMenu.razor`: logo + "Rodcast Solutions", íconos de **Bootstrap Icons** (instalada localmente en `wwwroot/lib/bootstrap-icons/`, sin CDN externo) distintos por sección, degradado de fondo basado en los colores del logo (`#1E88C7` → `#1B3E7D` → `#0F2A57`).

## Pendiente

- **Fase 5 — Despliegue**: Docker + Cloudflare Tunnel en el servidor casero. **No se ha empezado.**
- **Proyecto `per_ticket`** (HEM soporte por ticket): está armado y probado con datos de prueba, pero en la vida real "aún no arranca" según el diseño original — falta usarlo con datos reales cuando el cliente lo active.
- **Fuera de alcance explícito** (decisión ya tomada, no reabrir sin que el usuario lo pida): Cotizaciones (`QUOTES`), Dashboard de totales, Recordatorios de pago vencido, Facturación electrónica ante Hacienda.
- **Seguridad**: la contraseña real de MariaDB se compartió en texto plano en una conversación anterior — recomendado rotarla si no se ha hecho.
- **Opcional/cosmético**: quedan algunas sugerencias de estilo del analizador de C# (`IDE0290`, `IDE0305`, `CA1860`) sin aplicar — no son errores, se pueden ignorar o limpiar cuando haya tiempo.

## Gotchas ya resueltos (para no repetirlos)

- **Pomelo vs .NET 10**: hay que mantener `Pomelo.EntityFrameworkCore.MySql` y `Microsoft.EntityFrameworkCore.Tools` en `9.0.x` aunque el proyecto sea `net10.0` — Visual Studio a veces sugiere actualizar a 10.x, no aceptar esa actualización todavía.
- **Blazor Server "no specifies which form"**: hace falta `@rendermode="InteractiveServer"` explícito en `<Routes />` de `App.razor` para que la interactividad "Global" funcione de verdad; si no, los formularios fallan o no conservan los valores. También usar `<InputText>`/`<InputNumber>`/`<InputSelect>` (no `<input @bind>` normal) dentro de `EditForm`.
- **MySQL/MariaDB + migraciones de índices**: si una migración reemplaza un índice usado por una foreign key, hay que **crear el índice nuevo antes de borrar el viejo** (si no, error "needed in a foreign key constraint").
- **`Path.Combine` con rutas que empiezan en `/`**: en .NET, si el segundo argumento de `Path.Combine` empieza con `/` o `\`, ignora el primero por completo. Por eso `Pdf/WebRootFileReader.cs` recorta esos caracteres antes de combinar.
- **Razor + comillas anidadas en atributos**: evitar interpolación de strings (`$"..."`) directo dentro de un atributo `@onclick="..."` — mejor mover esa lógica a un método en `@code`.
- **QuestPDF**: `CornerRadius()` va después de `.Background()`; para separar celdas visualmente hay que poner `.Padding()` **antes** de `.Background()` (no después).

## Cómo continuar en una sesión nueva

1. Abrir la solución en Visual Studio (`RodcastInvoiceApp..slnx`).
2. Configurar la cadena de conexión real vía `user-secrets` si la sesión es en otra máquina.
3. Si hay cambios de modelo pendientes: `Add-Migration <nombre>` → revisar el archivo generado → `Update-Database`.
4. El siguiente paso lógico según el plan original es la **Fase 5 (despliegue)** — Docker + Cloudflare Tunnel.
