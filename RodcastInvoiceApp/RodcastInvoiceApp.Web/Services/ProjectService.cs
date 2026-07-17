using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using RodcastInvoiceApp.Web.Data;
using RodcastInvoiceApp.Web.DataTransferObjects.Project;
using RodcastInvoiceApp.Web.Exceptions;
using RodcastInvoiceApp.Web.Interfaces;

namespace RodcastInvoiceApp.Web.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly IValidator<ProjectCreateDto> _createValidator;
        private readonly IValidator<ProjectUpdateDto> _updateValidator;

        public ProjectService(
            AppDbContext context,
            IValidator<ProjectCreateDto> createValidator,
            IValidator<ProjectUpdateDto> updateValidator)
        {
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<ProjectResponseDto>> GetAllAsync(int? clientId = null)
        {
            var query = _context.Projects.AsNoTracking();

            if (clientId is not null)
                query = query.Where(p => p.ClientId == clientId);

            var projects = await query
                .Include(p => p.Client)
                .Include(p => p.PriceRules)
                .ToListAsync();

            return projects.Select(p => p.Adapt<ProjectResponseDto>());
        }

        public async Task<ProjectResponseDto> GetByIdAsync(int id)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .Include(p => p.Client)
                .Include(p => p.PriceRules)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException("Proyecto no encontrado.");

            return project.Adapt<ProjectResponseDto>();
        }

        public async Task<ProjectResponseDto> CreateAsync(ProjectCreateDto dto)
        {
            await ValidateAsync(_createValidator, dto);

            var clientExists = await _context.Clients.AnyAsync(c => c.Id == dto.ClientId);
            if (!clientExists)
                throw new NotFoundException("Cliente no encontrado.");

            await ValidateUniqueCodeAsync(dto.ClientId, dto.Code);

            var project = dto.Adapt<Data.Models.Project>();

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(project.Id);
        }

        public async Task<ProjectResponseDto> UpdateAsync(int id, ProjectUpdateDto dto)
        {
            await ValidateAsync(_updateValidator, dto);

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException("Proyecto no encontrado.");

            await ValidateUniqueCodeAsync(project.ClientId, dto.Code, id);

            dto.Adapt(project);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(project.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException("Proyecto no encontrado.");

            var hasInvoices = await _context.Invoices.AnyAsync(i => i.ProjectId == id);
            if (hasInvoices)
                throw new ConflictException(
                    "No se puede eliminar el proyecto porque tiene facturas asociadas.");

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }

        private static async Task ValidateAsync<T>(IValidator<T> validator, T dto)
        {
            var result = await validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new BadRequestException(
                    string.Join(" ", result.Errors.Select(e => e.ErrorMessage)));
        }

        private async Task ValidateUniqueCodeAsync(int clientId, string code, int? excludeId = null)
        {
            var codeExists = await _context.Projects
                .AnyAsync(p => p.ClientId == clientId && p.Code == code
                            && (excludeId == null || p.Id != excludeId));

            if (codeExists)
                throw new ConflictException("Ya existe un proyecto con ese código para este cliente.");
        }
    }
}
