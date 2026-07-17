using RodcastInvoiceApp.Web.Data.Models;

namespace RodcastInvoiceApp.Web.Billing
{
    public interface IBillingStrategy
    {
        BillingType BillingType { get; }

        // vatPercent se pasa aparte (no vive en Project ni en el input del mes/ticket)
        // porque el calculo del "monto fijo" del retainer mensual necesita conocerlo
        // para que Subtotal + VAT siempre de exactamente el monto del contrato.
        IReadOnlyList<InvoiceItemDraft> BuildInvoiceItems(Project project, BillingInput input, decimal vatPercent);
    }
}
