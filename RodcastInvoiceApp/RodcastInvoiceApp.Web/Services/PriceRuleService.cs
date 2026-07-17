using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using RodcastInvoiceApp.Web.Data;
using RodcastInvoiceApp.Web.DataTransferObjects.PriceRule;
using RodcastInvoiceApp.Web.Exceptions;
using RodcastInvoiceApp.Web.Interfaces;

namespace RodcastInvoiceApp.Web.Services
{
    public class PriceRuleService : IPriceRuleService
    {
        private readonly AppDbContext _context;
        private readonly IValidator<PriceRuleCreateDto> _createValidator;
        private readonly IValidator<PriceRuleUpdateDto> _updateValidator;

        public PriceRuleService(
            AppDbContext context,
            IValidator<PriceRuleCreateDto> createValidator,
            IValidator<PriceRuleUpdateDto> updateValidator)
        {
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<PriceRuleResponseDto>> GetAllAsync(int projectId)
        {
            return await _context.PriceRules
                .AsNoTracking()
                .Where(pr => pr.ProjectId == projectId)
                .ProjectToType<PriceRuleResponseDto>()
                .ToListAsync();
        }

        public async Task<PriceRuleResponseDto> CreateAsync(PriceRuleCreateDto dto)
        {
            await ValidateAsync(_createValidator, dto);

            var projectExists = await _context.Projects.AnyAsync(p => p.Id == dto.ProjectId);
            if (!projectExists)
                throw new NotFoundException("Proyecto no encontrado.");

            var priceRule = dto.Adapt<Data.Models.PriceRule>();

            _context.PriceRules.Add(priceRule);
            await _context.SaveChangesAsync();

            return priceRule.Adapt<PriceRuleResponseDto>();
        }

        public async Task<PriceRuleResponseDto> UpdateAsync(int id, PriceRuleUpdateDto dto)
        {
            await ValidateAsync(_updateValidator, dto);

            var priceRule = await _context.PriceRules
                .FirstOrDefaultAsync(pr => pr.Id == id)
                ?? throw new NotFoundException("Tarifa no encontrada.");

            dto.Adapt(priceRule);
            await _context.SaveChangesAsync();

            return priceRule.Adapt<PriceRuleResponseDto>();
        }

        public async Task DeleteAsync(int id)
        {
            var priceRule = await _context.PriceRules
                .FirstOrDefaultAsync(pr => pr.Id == id)
                ?? throw new NotFoundException("Tarifa no encontrada.");

            _context.PriceRules.Remove(priceRule);
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
