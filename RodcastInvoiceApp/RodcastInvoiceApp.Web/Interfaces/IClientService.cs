using RodcastInvoiceApp.Web.DataTransferObjects.Client;

namespace RodcastInvoiceApp.Web.Interfaces
{
    public interface IClientService
    {
        Task<IEnumerable<ClientResponseDto>> GetAllAsync(int? take = null);
        Task<ClientResponseDto> GetByIdAsync(int id);
        Task<ClientResponseDto> CreateAsync(ClientCreateDto dto);
        Task<ClientResponseDto> UpdateAsync(int id, ClientUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
