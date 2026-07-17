using System.ComponentModel.DataAnnotations;
using RodcastInvoiceApp.Web.Data.Common;

namespace RodcastInvoiceApp.Web.Data.Models
{
    public enum BillingType
    {
        MonthlyRetainer,
        PerTicket
    }

    public class Project : BaseEntity
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [StringLength(50)]
        public string? CostCenter { get; set; }

        public bool IsActive { get; set; } = true;

        public BillingType BillingType { get; set; }

        // Parametros especificos del proyecto en JSON crudo (monto de retainer,
        // multiplicador de hora extra, horas estandar del mes, etc.).
        // Se interpreta segun BillingType en la logica de facturacion (Fase 3).
        public string Config { get; set; } = "{}";

        public int ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;

        public virtual ICollection<PriceRule> PriceRules { get; set; } = new List<PriceRule>();
    }
}
