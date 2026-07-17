using RodcastInvoiceApp.Web.DataTransferObjects.PriceRule;

namespace RodcastInvoiceApp.Web.Interfaces
{
    public interface IPriceRuleService
    {
        Task<IEnumerable<PriceRuleResponseDto>> GetAllAsync(int projectId);
        Task<PriceRuleResponseDto> CreateAsync(PriceRuleCreateDto dto);
        Task<PriceRuleResponseDto> UpdateAsync(int id, PriceRuleUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
