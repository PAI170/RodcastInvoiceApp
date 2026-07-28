using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using RodcastInvoiceApp.Web.Data;
using RodcastInvoiceApp.Web.DataTransferObjects.Project;
using RodcastInvoiceApp.Web.Exceptions;
using RodcastInvoiceApp.Web.Interfaces;
using RodcastInvoiceApp.Web.Resources;
using RodcastInvoiceApp.Web.Security;

namespace RodcastInvoiceApp.Web.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly IValidator<ProjectCreateDto> _createValidator;
        private readonly IValidator<ProjectUpdateDto> _updateValidator;
        private readonly ICurrentUserAccessor _currentUser;
        private readonly IStringLocalizer<SharedResource> _loc;

        public ProjectService(
            AppDbContext context,
            IValidator<ProjectCreateDto> createValidator,
            IValidator<ProjectUpdateDto> updateValidator,
            ICurrentUserAccessor currentUser,
            IStringLocalizer<SharedResource> loc)
        {
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _currentUser = currentUser;
            _loc = loc;
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
                ?? throw new NotFoundException(_loc["SvcErr_ProjectNotFound"]);

            return project.Adapt<ProjectResponseDto>();
        }

        public async Task<ProjectResponseDto> CreateAsync(ProjectCreateDto dto)
        {
            await _currentUser.EnsureAdminAsync();
            await ValidateAsync(_createValidator, dto);

            var clientExists = await _context.Clients.AnyAsync(c => c.Id == dto.ClientId);
            if (!clientExists)
                throw new NotFoundException(_loc["SvcErr_ClientNotFound"]);

            await ValidateUniqueCodeAsync(dto.ClientId, dto.Code);

            var project = dto.Adapt<Data.Models.Project>();

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(project.Id);
        }

        public async Task<ProjectResponseDto> UpdateAsync(int id, ProjectUpdateDto dto)
        {
            await _currentUser.EnsureAdminAsync();
            await ValidateAsync(_updateValidator, dto);

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException(_loc["SvcErr_ProjectNotFound"]);

            await ValidateUniqueCodeAsync(project.ClientId, dto.Code, id);

            dto.Adapt(project);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(project.Id);
        }

        public async Task DeleteAsync(int id)
        {
            await _currentUser.EnsureAdminAsync();

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException(_loc["SvcErr_ProjectNotFound"]);

            var hasInvoices = await _context.Invoices.AnyAsync(i => i.ProjectId == id);
            if (hasInvoices)
                throw new ConflictException(
                    _loc["SvcErr_ProjectHasInvoices"]);

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
                throw new ConflictException(_loc["SvcErr_ProjectDuplicateCode"]);
        }
    }
}
