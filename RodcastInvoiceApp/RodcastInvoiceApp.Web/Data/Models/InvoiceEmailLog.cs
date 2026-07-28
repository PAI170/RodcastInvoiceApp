using RodcastInvoiceApp.Web.Data.Common;

namespace RodcastInvoiceApp.Web.Data.Models
{
    // Una fila por cada intento real de mandar (o re-archivar en Sent) el correo
    // de una factura. Append-only: nunca se edita ni se pisa, es el historial.
    // CreatedAt (de BaseEntity) es la fecha/hora del envio.
    public class InvoiceEmailLog : BaseEntity
    {
        public int InvoiceId { get; set; }
        public virtual Invoice Invoice { get; set; } = null!;

        public string SentByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser SentByUser { get; set; } = null!;

        public string ToEmail { get; set; } = string.Empty;

        // Si el APPEND a la carpeta Sent via IMAP funciono. False en un envio
        // normal no significa que el correo no llego al cliente (eso es SMTP,
        // separado) - solo que no se pudo guardar copia en Sent.
        public bool SavedToSentFolder { get; set; }

        // True = esta fila vino de "Reintentar guardado en Sent" (no se mando
        // nada por SMTP). False = envio real del correo al cliente.
        public bool IsRetry { get; set; }
    }
}
