using RodcastInvoiceApp.Web.DataTransferObjects.BankAccount;

namespace RodcastInvoiceApp.Web.Interfaces
{
    public interface IBankAccountService
    {
        Task<IEnumerable<BankAccountResponseDto>> GetAllAsync();
        Task<BankAccountResponseDto> GetByIdAsync(int id);
        Task<BankAccountResponseDto> CreateAsync(BankAccountCreateDto dto);
        Task<BankAccountResponseDto> UpdateAsync(int id, BankAccountUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
