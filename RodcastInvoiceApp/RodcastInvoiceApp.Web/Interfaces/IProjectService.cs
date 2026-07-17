using RodcastInvoiceApp.Web.DataTransferObjects.Project;

namespace RodcastInvoiceApp.Web.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectResponseDto>> GetAllAsync(int? clientId = null);
        Task<ProjectResponseDto> GetByIdAsync(int id);
        Task<ProjectResponseDto> CreateAsync(ProjectCreateDto dto);
        Task<ProjectResponseDto> UpdateAsync(int id, ProjectUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
