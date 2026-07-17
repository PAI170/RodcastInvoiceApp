using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using RodcastInvoiceApp.Web.Data;
using RodcastInvoiceApp.Web.DataTransferObjects.BankAccount;
using RodcastInvoiceApp.Web.Exceptions;
using RodcastInvoiceApp.Web.Interfaces;
using RodcastInvoiceApp.Web.Security;

namespace RodcastInvoiceApp.Web.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly AppDbContext _context;
        private readonly IValidator<BankAccountCreateDto> _createValidator;
        private readonly IValidator<BankAccountUpdateDto> _updateValidator;
        private readonly ICurrentUserAccessor _currentUser;

        public BankAccountService(
            AppDbContext context,
            IValidator<BankAccountCreateDto> createValidator,
            IValidator<BankAccountUpdateDto> updateValidator,
            ICurrentUserAccessor currentUser)
        {
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _currentUser = currentUser;
        }

        public async Task<IEnumerable<BankAccountResponseDto>> GetAllAsync()
        {
            return await _context.BankAccounts
                .AsNoTracking()
                .ProjectToType<BankAccountResponseDto>()
                .ToListAsync();
        }

        public async Task<BankAccountResponseDto> GetByIdAsync(int id)
        {
            var bankAccount = await _context.BankAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id)
                ?? throw new NotFoundException("Cuenta bancaria no encontrada.");

            return bankAccount.Adapt<BankAccountResponseDto>();
        }

        public async Task<BankAccountResponseDto> CreateAsync(BankAccountCreateDto dto)
        {
            await _currentUser.EnsureAdminAsync();
            await ValidateAsync(_createValidator, dto);

            var bankAccount = dto.Adapt<Data.Models.BankAccount>();

            _context.BankAccounts.Add(bankAccount);
            await _context.SaveChangesAsync();

            return bankAccount.Adapt<BankAccountResponseDto>();
        }

        public async Task<BankAccountResponseDto> UpdateAsync(int id, BankAccountUpdateDto dto)
        {
            await _currentUser.EnsureAdminAsync();
            await ValidateAsync(_updateValidator, dto);

            var bankAccount = await _context.BankAccounts
                .FirstOrDefaultAsync(b => b.Id == id)
                ?? throw new NotFoundException("Cuenta bancaria no encontrada.");

            dto.Adapt(bankAccount);
            await _context.SaveChangesAsync();

            return bankAccount.Adapt<BankAccountResponseDto>();
        }

        public async Task DeleteAsync(int id)
        {
            await _currentUser.EnsureAdminAsync();

            var bankAccount = await _context.BankAccounts
                .FirstOrDefaultAsync(b => b.Id == id)
                ?? throw new NotFoundException("Cuenta bancaria no encontrada.");

            var hasInvoices = await _context.Invoices.AnyAsync(i => i.BankAccountId == id);
            if (hasInvoices)
                throw new ConflictException(
                    "No se puede eliminar la cuenta bancaria porque tiene facturas asociadas.");

            _context.BankAccounts.Remove(bankAccount);
            await _context.SaveChangesAsync();
        }

        private static async Task ValidateAsync<T>(IValidator<T> validator, T dto)
        {
            var result = await validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new BadRequestException(
                    string.Join(" ", result.Errors.Select(e => e.ErrorMessage)));
        }
    }
}
