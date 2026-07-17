using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using RodcastInvoiceApp.Web.Billing;
using RodcastInvoiceApp.Web.Data;
using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.DataTransferObjects.Invoice;
using RodcastInvoiceApp.Web.DataTransferObjects.Payment;
using RodcastInvoiceApp.Web.Exceptions;
using RodcastInvoiceApp.Web.Interfaces;
using RodcastInvoiceApp.Web.Security;

namespace RodcastInvoiceApp.Web.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _context;
        private readonly IValidator<InvoiceCreateDto> _invoiceValidator;
        private readonly IValidator<PaymentCreateDto> _paymentValidator;
        private readonly IEnumerable<IBillingStrategy> _billingStrategies;
        private readonly ICurrentUserAccessor _currentUser;

        public InvoiceService(
            AppDbContext context,
            IValidator<InvoiceCreateDto> invoiceValidator,
            IValidator<PaymentCreateDto> paymentValidator,
            IEnumerable<IBillingStrategy> billingStrategies,
            ICurrentUserAccessor currentUser)
        {
            _context = context;
            _invoiceValidator = invoiceValidator;
            _paymentValidator = paymentValidator;
            _billingStrategies = billingStrategies;
            _currentUser = currentUser;
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetAllAsync(
            int? projectId = null, int? take = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = IncludeAll(_context.Invoices.AsNoTracking());

            if (projectId is not null)
                query = query.Where(i => i.ProjectId == projectId);

            if (fromDate is not null)
                query = query.Where(i => i.InvoiceDate >= fromDate);

            if (toDate is not null)
                query = query.Where(i => i.InvoiceDate < toDate.Value.AddDays(1));

            IQueryable<Invoice> orderedQuery = query.OrderByDescending(i => i.InvoiceDate);

            if (take is not null)
                orderedQuery = orderedQuery.Take(take.Value);

            var invoices = await orderedQuery.ToListAsync();

            return invoices.Select(ToResponseDto);
        }

        public async Task<InvoiceResponseDto> GetByIdAsync(int id)
        {
            var invoice = await IncludeAll(_context.Invoices.AsNoTracking())
                .FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new NotFoundException("Factura no encontrada.");

            return ToResponseDto(invoice);
        }

        public async Task<InvoiceResponseDto> CreateAsync(InvoiceCreateDto dto)
        {
            var result = await _invoiceValidator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new BadRequestException(
                    string.Join(" ", result.Errors.Select(e => e.ErrorMessage)));

            var project = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.PriceRules)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId)
                ?? throw new NotFoundException("Proyecto no encontrado.");

            var bankAccountExists = await _context.BankAccounts.AnyAsync(b => b.Id == dto.BankAccountId);
            if (!bankAccountExists)
                throw new NotFoundException("Cuenta bancaria no encontrada.");

            await EnsureInvoiceNumberIsUniqueAsync(dto.InvoiceNumber);

            var strategy = _billingStrategies.FirstOrDefault(s => s.BillingType == project.BillingType)
                ?? throw new InvalidOperationException(
                    $"No hay una estrategia de facturación registrada para '{project.BillingType}'.");

            var billingInput = BuildBillingInput(project.BillingType, dto);
            var itemDrafts = strategy.BuildInvoiceItems(project, billingInput, dto.VatPercent);

            var invoice = new Invoice
            {
                ProjectId = dto.ProjectId,
                BankAccountId = dto.BankAccountId,
                InvoiceNumber = dto.InvoiceNumber,
                InvoiceDate = dto.InvoiceDate,
                DueDate = dto.DueDate,
                Currency = dto.Currency,
                VatPercent = dto.VatPercent,
                IsVatExonerated = dto.IsVatExonerated,
                PaymentMethod = dto.PaymentMethod,
                TicketNumber = dto.TicketNumber,
                City = dto.City,
                SlaType = dto.SlaType,
                AdditionalMinutes = project.BillingType == BillingType.PerTicket
                    ? dto.ApprovedAdditionalMinutes
                    : null,
                VacationDays = dto.VacationDays,
                WorkedDays = dto.WorkedDays,
                OvertimeHoursToInvoice = dto.OvertimeHoursToInvoice,
                InvoiceItems = itemDrafts.Select(d => new InvoiceItem
                {
                    Description = d.Description,
                    Quantity = d.Quantity,
                    Unit = d.Unit,
                    Rate = d.Rate,
                    Amount = d.Amount
                }).ToList()
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(invoice.Id);
        }

        public async Task<InvoiceResponseDto> UpdateAsync(int id, InvoiceCreateDto dto)
        {
            await _currentUser.EnsureAdminAsync();

            var result = await _invoiceValidator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new BadRequestException(
                    string.Join(" ", result.Errors.Select(e => e.ErrorMessage)));

            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new NotFoundException("Factura no encontrada.");

            if (invoice.Status != InvoiceStatus.Draft || invoice.Payments.Any())
                throw new ConflictException(
                    "No se puede editar una factura que ya fue enviada, pagada, o que tiene pagos registrados.");

            // El proyecto de una factura no se puede cambiar al editar (dto.ProjectId se ignora aqui).
            var project = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.PriceRules)
                .FirstOrDefaultAsync(p => p.Id == invoice.ProjectId)
                ?? throw new NotFoundException("Proyecto no encontrado.");

            var bankAccountExists = await _context.BankAccounts.AnyAsync(b => b.Id == dto.BankAccountId);
            if (!bankAccountExists)
                throw new NotFoundException("Cuenta bancaria no encontrada.");

            await EnsureInvoiceNumberIsUniqueAsync(dto.InvoiceNumber, excludeInvoiceId: id);

            var strategy = _billingStrategies.FirstOrDefault(s => s.BillingType == project.BillingType)
                ?? throw new InvalidOperationException(
                    $"No hay una estrategia de facturación registrada para '{project.BillingType}'.");

            var billingInput = BuildBillingInput(project.BillingType, dto);
            var itemDrafts = strategy.BuildInvoiceItems(project, billingInput, dto.VatPercent);

            invoice.BankAccountId = dto.BankAccountId;
            invoice.InvoiceNumber = dto.InvoiceNumber;
            invoice.InvoiceDate = dto.InvoiceDate;
            invoice.DueDate = dto.DueDate;
            invoice.Currency = dto.Currency;
            invoice.VatPercent = dto.VatPercent;
            invoice.IsVatExonerated = dto.IsVatExonerated;
            invoice.PaymentMethod = dto.PaymentMethod;
            invoice.TicketNumber = dto.TicketNumber;
            invoice.City = dto.City;
            invoice.SlaType = dto.SlaType;
            invoice.AdditionalMinutes = project.BillingType == BillingType.PerTicket
                ? dto.ApprovedAdditionalMinutes
                : null;
            invoice.VacationDays = dto.VacationDays;
            invoice.WorkedDays = dto.WorkedDays;
            invoice.OvertimeHoursToInvoice = dto.OvertimeHoursToInvoice;

            // Reemplaza las lineas con las recalculadas (las viejas ya no aplican).
            _context.InvoiceItems.RemoveRange(invoice.InvoiceItems);
            invoice.InvoiceItems = itemDrafts.Select(d => new InvoiceItem
            {
                Description = d.Description,
                Quantity = d.Quantity,
                Unit = d.Unit,
                Rate = d.Rate,
                Amount = d.Amount
            }).ToList();

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<InvoiceResponseDto> UpdateStatusAsync(int id, InvoiceStatus status)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new NotFoundException("Factura no encontrada.");

            // Una vez pagada, la factura queda cerrada: no se puede volver a cambiar el estado.
            if (invoice.Status == InvoiceStatus.Paid)
                throw new ConflictException("Esta factura ya está pagada, no se puede cambiar su estado.");

            invoice.Status = status;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task DeleteAsync(int id)
        {
            await _currentUser.EnsureAdminAsync();

            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new NotFoundException("Factura no encontrada.");

            if (invoice.Status != InvoiceStatus.Draft || invoice.Payments.Any())
                throw new ConflictException(
                    "No se puede eliminar una factura que ya fue enviada, pagada, o que tiene pagos registrados.");

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task<PaymentResponseDto> AddPaymentAsync(PaymentCreateDto dto)
        {
            var result = await _paymentValidator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new BadRequestException(
                    string.Join(" ", result.Errors.Select(e => e.ErrorMessage)));

            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == dto.InvoiceId)
                ?? throw new NotFoundException("Factura no encontrada.");

            var payment = dto.Adapt<Payment>();
            _context.Payments.Add(payment);

            // Si con este pago se cubre el total, se marca la factura como pagada automaticamente.
            var subtotal = invoice.InvoiceItems.Sum(i => i.Amount);
            var total = subtotal + Math.Round(subtotal * invoice.VatPercent / 100m, 2, MidpointRounding.AwayFromZero);
            var amountPaidAfter = invoice.Payments.Sum(p => p.Amount) + dto.Amount;

            if (amountPaidAfter >= total && invoice.Status != InvoiceStatus.Paid)
                invoice.Status = InvoiceStatus.Paid;

            await _context.SaveChangesAsync();

            return payment.Adapt<PaymentResponseDto>();
        }

        private async Task EnsureInvoiceNumberIsUniqueAsync(string invoiceNumber, int? excludeInvoiceId = null)
        {
            var query = _context.Invoices.Where(i => i.InvoiceNumber == invoiceNumber);
            if (excludeInvoiceId is not null)
                query = query.Where(i => i.Id != excludeInvoiceId);

            if (await query.AnyAsync())
                throw new ConflictException($"Ya existe una factura con el número '{invoiceNumber}'.");
        }

        private static BillingInput BuildBillingInput(BillingType billingType, InvoiceCreateDto dto)
        {
            switch (billingType)
            {
                case BillingType.MonthlyRetainer:
                    return new MonthlyRetainerInput
                    {
                        Month = dto.InvoiceDate.Month,
                        Year = dto.InvoiceDate.Year,
                        VacationDays = dto.VacationDays,
                        WorkedDays = dto.WorkedDays,
                        OvertimeHoursToInvoice = dto.OvertimeHoursToInvoice
                    };

                case BillingType.PerTicket:
                    if (string.IsNullOrWhiteSpace(dto.TicketNumber)
                        || string.IsNullOrWhiteSpace(dto.City)
                        || string.IsNullOrWhiteSpace(dto.SlaType))
                        throw new BadRequestException(
                            "Para un proyecto por ticket debes indicar número de ticket, ciudad y tipo de SLA.");

                    return new PerTicketInput
                    {
                        TicketNumber = dto.TicketNumber,
                        City = dto.City,
                        SlaType = dto.SlaType,
                        ApprovedAdditionalMinutes = dto.ApprovedAdditionalMinutes,
                        OvertimeHours = dto.OvertimeHoursToInvoice
                    };

                default:
                    throw new InvalidOperationException($"Tipo de cobro no soportado: {billingType}.");
            }
        }

        private static IQueryable<Invoice> IncludeAll(IQueryable<Invoice> query)
        {
            return query
                .Include(i => i.Project).ThenInclude(p => p.Client)
                .Include(i => i.BankAccount)
                .Include(i => i.InvoiceItems)
                .Include(i => i.Payments);
        }

        private static InvoiceResponseDto ToResponseDto(Invoice invoice)
        {
            var subtotal = invoice.InvoiceItems.Sum(i => i.Amount);
            var vatAmount = Math.Round(subtotal * invoice.VatPercent / 100m, 2, MidpointRounding.AwayFromZero);
            var total = subtotal + vatAmount;
            var amountPaid = invoice.Payments.Sum(p => p.Amount);

            return new InvoiceResponseDto
            {
                Id = invoice.Id,
                ProjectId = invoice.ProjectId,
                ProjectName = invoice.Project.Name,
                ProjectCostCenter = invoice.Project.CostCenter,
                ClientName = invoice.Project.Client.Name,
                ClientAddress = invoice.Project.Client.Address,
                ClientVatId = invoice.Project.Client.VatId,
                ClientSupplierIdAssigned = invoice.Project.Client.SupplierIdAssigned,
                BankAccountId = invoice.BankAccountId,
                BankAccountName = invoice.BankAccount.BankName,
                BankAccountIban = invoice.BankAccount.Iban,
                BankAccountSwift = invoice.BankAccount.Swift,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                Currency = invoice.Currency,
                VatPercent = invoice.VatPercent,
                IsVatExonerated = invoice.IsVatExonerated,
                Status = invoice.Status,
                PaymentMethod = invoice.PaymentMethod,
                TicketNumber = invoice.TicketNumber,
                City = invoice.City,
                SlaType = invoice.SlaType,
                AdditionalMinutes = invoice.AdditionalMinutes,
                VacationDays = invoice.VacationDays,
                WorkedDays = invoice.WorkedDays,
                OvertimeHoursToInvoice = invoice.OvertimeHoursToInvoice,
                HasTimesheet = invoice.TimesheetExceptions is not null,
                Items = invoice.InvoiceItems.Select(i => i.Adapt<InvoiceItemResponseDto>()).ToList(),
                Payments = invoice.Payments.Select(p => p.Adapt<PaymentResponseDto>()).ToList(),
                Subtotal = subtotal,
                VatAmount = vatAmount,
                Total = total,
                AmountPaid = amountPaid,
                Balance = total - amountPaid,
                CreatedAt = invoice.CreatedAt,
                UpdatedAt = invoice.UpdatedAt
            };
        }
    }
}
