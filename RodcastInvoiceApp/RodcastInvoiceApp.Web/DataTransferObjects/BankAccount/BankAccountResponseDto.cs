namespace RodcastInvoiceApp.Web.DataTransferObjects.BankAccount
{
    public class BankAccountResponseDto
    {
        public int Id { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountHolder { get; set; } = string.Empty;
        public string? AccountNumber { get; set; }
        public string? Iban { get; set; }
        public string? Swift { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
