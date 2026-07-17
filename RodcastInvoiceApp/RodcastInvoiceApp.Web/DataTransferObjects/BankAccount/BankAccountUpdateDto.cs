namespace RodcastInvoiceApp.Web.DataTransferObjects.BankAccount
{
    public class BankAccountUpdateDto
    {
        public string BankName { get; set; } = string.Empty;
        public string AccountHolder { get; set; } = string.Empty;
        public string? AccountNumber { get; set; }
        public string? Iban { get; set; }
        public string? Swift { get; set; }
        public string Currency { get; set; } = "USD";
    }
}
