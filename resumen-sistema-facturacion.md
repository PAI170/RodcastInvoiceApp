# Sistema de facturación interna — Rodcast Solutions

Resumen de diseño para retomar contexto si se necesita abrir un chat nuevo con Claude.

## Objetivo

Sistema de **registro interno** de facturación (NO facturación electrónica ante Hacienda) para evitar hacer facturas a mano y llevar control de clientes, proyectos y facturas. Actualmente 2 proyectos, ambos con el mismo cliente: **HEM (Hemmersbach GmbH & Co. KG)**. Se espera que en el futuro lleguen más clientes.

## Stack tecnológico decidido

- **Backend + Frontend**: ASP.NET Core + Blazor Server (un solo lenguaje, C#, que es lo que el usuario domina — no Node.js)
- **ORM**: Entity Framework Core
- **Base de datos**: MariaDB (reusa la instancia que ya corre en el servidor propio vía CloudPanel, evita levantar infraestructura nueva) usando el proveedor `Pomelo.EntityFrameworkCore.MySql`
- **PDF**: QuestPDF (gratis para uso interno/pequeñas empresas)
- **Despliegue**: mismo patrón que sus otros sitios — Docker container + Cloudflare Tunnel en su servidor propio
- Nota aclarada: T-SQL es exclusivo de SQL Server; MariaDB usa su propio dialecto. Como se usará EF Core (LINQ), casi no se escribirá SQL crudo, así que el cambio de sintaxis no es un problema real.

## Modelo de datos (entidades principales)

### CLIENTS
Empresas a las que se factura. Aunque hoy solo hay un cliente (HEM), se modela como tabla completa pensando en clientes futuros.
- id, name, address, country, vat_id
- **supplier_id_assigned**: el ID que el CLIENTE le asigna a Rodcast como proveedor (ej. "001351" que HEM le pidió agregar) — no es un dato de Rodcast, vive en el cliente que lo asignó
- default_currency

### PROJECTS
Cada contrato/proyecto de un cliente. Es la pieza clave de la arquitectura extensible.
- id, client_id (FK), name, code, cost_center (nullable, algunos clientes lo piden y otros no), is_active
- **billing_type**: identificador del tipo de cobro (`monthly_retainer`, `per_ticket`, y los que vengan)
- **config** (JSON): parámetros específicos del proyecto sin necesidad de agregar columnas nuevas cada vez (ej. monto de retainer, multiplicador de hora extra, horas estándar del mes)

### PRICE_RULES (generalización de lo que antes se llamó RATE_CARDS)
Tabla de tarifas genérica y reutilizable para cualquier proyecto con precios por categoría (ciudad, tipo de SLA, tipo de dispositivo, lo que sea en el futuro).
- id, project_id (FK), dimension_1, dimension_2 (nullable), rate, label
- Hoy se usa como: dimension_1 = ciudad, dimension_2 = tipo de SLA (SBD/NBD), rate = tarifa

### INVOICES
Encabezado de cada factura.
- id, project_id (FK), bank_account_id (FK)
- **invoice_number** (texto libre — preserva numeración histórica de HEM que ya iba 001-012; a partir de ahora se sugiere formato `{CódigoProyecto}-{Año}-{Secuencial}`, editable)
- invoice_date, due_date, currency (solo USD por ahora), vat_percent (13% CR por defecto)
- **is_vat_exonerated** (bool) — bandera visual/reporte; el cálculo real siempre usa vat_percent (se pone en 0 cuando aplica)
- status (borrador/enviada/pagada/vencida), payment_method
- ticket_number, city, sla_type, additional_minutes (solo aplican a proyectos per_ticket)

### INVOICE_ITEMS
Líneas de detalle de cada factura (1 o varias según el mes/ticket).
- id, invoice_id (FK), description, quantity, unit (horas/tickets/etc.), rate

### BANK_ACCOUNTS
Cuentas para cobro (banco, SWIFT, IBAN, moneda).

### PAYMENTS
Registro de pagos recibidos por factura (para llevar control de pendiente vs. pagado).

### COMPANY_SETTINGS
Tabla de una sola fila con los datos fijos de Rodcast (nombre, dirección, Tax ID, logo, firma) para generar el PDF.

## Lógica de negocio por tipo de facturación

### `monthly_retainer` (Proyecto 1 — HEM, soporte mensual)

Tarifa base fija: **$10.22/hora**. Retainer contractual: **$2000/mes con VAT incluido**.

| Escenario | Cálculo de la línea |
|---|---|
| Mes normal (sin vacaciones) | Quantity = `1769.91 ÷ 10.22 ≈ 173.18h` (calculado hacia atrás), Rate = $10.22 → Subtotal $1769.91 + 13% VAT = **$2000** exacto. Las horas se muestran en el detalle de la factura aunque el monto sea fijo. |
| Mes con vacaciones | Quantity = `días_trabajados × 8`, Rate = $10.22. Ejemplo real ya facturado: 20 días trabajados (3 días de vacaciones) × 8h = 160h × $10.22 = $1635.20 + VAT = $1847.78 |
| Horas extra | Línea aparte, se factura el mes siguiente (no se acumula ni se trunca ningún tope — esa lógica de "saldo acumulado" que se propuso al inicio quedó descartada). Rate = $10.22 × 1.5 = **$15.33/hora** |

Importante: **NO hay lógica de tope que trunque ni acumule saldo de horas.** Es simplemente: mes normal = $2000 fijo, mes con vacaciones = por horas trabajadas, hora extra = línea aparte al 1.5x el mes siguiente.

Formulario de captura mensual: mes/año, días de vacaciones (0 = dispara cálculo de $2000 fijo), días trabajados (solo si vacaciones > 0), horas extra a facturar del mes anterior.

### `per_ticket` (Proyecto 2 — HEM, soporte por ticket, aún no arranca)

Basado en el "Pricing agreement" (sección 17) que el cliente ya envió. Tarifas por ciudad y tipo de SLA, en Costa Rica:

| Ciudad | 4HR SBD (primera hora) | NBD Response (primera hora) | Hora adicional |
|---|---|---|---|
| Ulloa, Heredia | $70.00 | $45.00 | $25.00 |
| Liberia | $100.00 | $70.00 | $25.00 |
| Moravia | $70.00 | $45.00 | $25.00 |
| Alajuela | $90.00 | $70.00 | $25.00 |
| San José | $70.00 | $45.00 | $25.00 |
| Heredia | $70.00 | $45.00 | $25.00 |

Reglas:
- Precios por ticket, incluyen traslado/transporte, excluyen VAT
- La tarifa base incluye la primera hora completa
- Tiempo adicional se factura en incrementos de **15 minutos** a la tarifa de "hora adicional" ($25 fijo en todas las ciudades)
- El tiempo adicional **solo se factura si el SDM lo aprobó** — si no hay aprobación, esa parte NO se registra en el sistema (decisión explícita: no se guarda como no-facturable, simplemente no se ingresa)
- Se asume: **1 ticket = 1 factura** (a confirmar cuando el proyecto arranque realmente, aún no hay tickets reales trabajados)
- Hora extra (si llegara a haber, poco probable): mismo esquema 1.5x que el otro proyecto

## Arquitectura para futuros proyectos/clientes

Separación clara entre **datos** (100% genéricos, no requieren tocar el schema) y **lógica de cálculo** (requiere una clase de código nueva solo cuando el modelo de cobro es genuinamente distinto a los que ya existen):

- `PROJECTS.config` (JSON) + `PRICE_RULES` cubren cualquier combinación de tarifas/parámetros sin migraciones nuevas
- Cada `billing_type` se implementa como una clase que sigue una interfaz común (`IBillingStrategy`): recibe el proyecto + datos del mes/ticket, devuelve las líneas de factura
- Un cliente/proyecto nuevo que cobra igual que uno existente → cero código, solo cargar datos
- Un modelo de cobro genuinamente nuevo → una clase nueva (~30-60 min), sin tocar lo que ya funciona

No se preguntará "¿es factura normal o por ticket?" al usuario — se infiere automáticamente del `billing_type` del proyecto seleccionado.

## Fuera de alcance por ahora (implementar después, sin refactor)

- **Cotizaciones** (`QUOTES` + `QUOTE_ITEMS`, mismo patrón encabezado+líneas que facturas, con botón "convertir a factura")
- Dashboard de totales facturados por mes/año
- Recordatorios de pago vencido
- Facturación electrónica ante Hacienda (fuera de alcance total — esto es solo registro interno)

## Decisiones ya cerradas (no volver a preguntar)

- Numeración: texto libre por factura, formato sugerido `{CódigoProyecto}-{Año}-{Secuencial}`, se preserva el historial 001-012 tal cual
- Solo USD por ahora
- VAT 13% Costa Rica por defecto, con opción de exoneración (0%, sin campo de referencia/autorización)
- No hay lógica de acumulación de horas ni tope truncado — fue una idea descartada
- Tiempo adicional no aprobado por SDM: no se registra en el sistema

## Próximo paso pendiente

Empezar el scaffold del proyecto en C# (ASP.NET Core + Blazor Server + EF Core) con el alcance de: Clientes, Proyectos, Facturas (ambos tipos de cobro), generación de PDF replicando la plantilla actual de Rodcast, numeración sugerida. Cotizaciones y dashboard quedan fuera del scaffold inicial.
