using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using RodcastInvoiceApp.Web.Data;
using RodcastInvoiceApp.Web.DataTransferObjects.Client;
using RodcastInvoiceApp.Web.Exceptions;
using RodcastInvoiceApp.Web.Interfaces;

namespace RodcastInvoiceApp.Web.Services
{
    public class ClientService : IClientService
    {
        private readonly AppDbContext _context;
        private readonly IValidator<ClientCreateDto> _createValidator;
        private readonly IValidator<ClientUpdateDto> _updateValidator;

        public ClientService(
            AppDbContext context,
            IValidator<ClientCreateDto> createValidator,
            IValidator<ClientUpdateDto> updateValidator)
        {
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<ClientResponseDto>> GetAllAsync(int? take = null)
        {
            IQueryable<Data.Models.Client> query = _context.Clients.AsNoTracking();

            // Solo se ordena/limita cuando se pide un "take" (ej. clientes recientes en el Home),
            // para no cambiar el orden de la lista completa en /clients.
            if (take is not null)
                query = query.OrderByDescending(c => c.CreatedAt).Take(take.Value);

            return await query.ProjectToType<ClientResponseDto>().ToListAsync();
        }

        public async Task<ClientResponseDto> GetByIdAsync(int id)
        {
            var client = await _context.Clients
                .AsNoTracking()
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Cliente no encontrado.");

            return client.Adapt<ClientResponseDto>();
        }

        public async Task<ClientResponseDto> CreateAsync(ClientCreateDto dto)
        {
            await ValidateAsync(_createValidator, dto);
            await ValidateUniqueFieldsAsync(dto.Name, dto.VatId);

            var client = dto.Adapt<Data.Models.Client>();

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return client.Adapt<ClientResponseDto>();
        }

        public async Task<ClientResponseDto> UpdateAsync(int id, ClientUpdateDto dto)
        {
            await ValidateAsync(_updateValidator, dto);

            var client = await _context.Clients
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Cliente no encontrado.");

            await ValidateUniqueFieldsAsync(dto.Name, dto.VatId, id);

            dto.Adapt(client);
            await _context.SaveChangesAsync();

            return client.Adapt<ClientResponseDto>();
        }

        public async Task DeleteAsync(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Cliente no encontrado.");

            if (client.Projects.Any())
                throw new ConflictException(
                    "No se puede eliminar el cliente porque tiene proyectos asociados.");

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }

        private static async Task ValidateAsync<T>(IValidator<T> validator, T dto)
        {
            var result = await validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new BadRequestException(
                    string.Join(" ", result.Errors.Select(e => e.ErrorMessage)));
        }

        private async Task ValidateUniqueFieldsAsync(string name, string vatId, int? excludeId = null)
        {
            var nameExists = await _context.Clients
                .AnyAsync(c => c.Name == name && (excludeId == null || c.Id != excludeId));

            if (nameExists)
                throw new ConflictException("Ya existe un cliente con ese nombre.");

            var vatIdExists = await _context.Clients
                .AnyAsync(c => c.VatId == vatId && (excludeId == null || c.Id != excludeId));

            if (vatIdExists)
                throw new ConflictException("Ya existe un cliente con ese VAT ID.");
        }
    }
}
