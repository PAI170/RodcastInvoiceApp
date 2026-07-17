using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using RodcastInvoiceApp.Web.Data;
using RodcastInvoiceApp.Web.DataTransferObjects.CompanySettings;
using RodcastInvoiceApp.Web.Exceptions;
using RodcastInvoiceApp.Web.Interfaces;

namespace RodcastInvoiceApp.Web.Services
{
    public class CompanySettingsService : ICompanySettingsService
    {
        private readonly AppDbContext _context;
        private readonly IValidator<CompanySettingsDto> _validator;

        public CompanySettingsService(AppDbContext context, IValidator<CompanySettingsDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<CompanySettingsDto> GetAsync()
        {
            var settings = await GetOrCreateEntityAsync();
            return settings.Adapt<CompanySettingsDto>();
        }

        public async Task<CompanySettingsDto> UpdateAsync(CompanySettingsDto dto)
        {
            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new BadRequestException(
                    string.Join(" ", result.Errors.Select(e => e.ErrorMessage)));

            var settings = await GetOrCreateEntityAsync();
            dto.Adapt(settings);
            await _context.SaveChangesAsync();

            return settings.Adapt<CompanySettingsDto>();
        }

        private async Task<Data.Models.CompanySettings> GetOrCreateEntityAsync()
        {
            var settings = await _context.CompanySettings.FirstOrDefaultAsync();
            if (settings is not null)
                return settings;

            settings = new Data.Models.CompanySettings();
            _context.CompanySettings.Add(settings);
            await _context.SaveChangesAsync();

            return settings;
        }
    }
}
