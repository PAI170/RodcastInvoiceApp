using System.Net;

namespace RodcastInvoiceApp.Web.Security
{
    // Arma el HtmlBody de los correos de factura: mensaje + firma. Un solo lugar
    // para esta logica, usada tanto al mandar el correo de verdad (MailKitEmailSender)
    // como en la vista previa de la pantalla de aprobaciones, para que el admin vea
    // exactamente lo mismo que le va a llegar al cliente.
    public static class EmailHtmlRenderer
    {
        public static string BuildHtmlBody(string body, string? signatureHtml)
        {
            var htmlMessage = WebUtility.HtmlEncode(body).Replace("\n", "<br/>");
            var messageDiv = $"<div style=\"font-family: Verdana, Geneva, sans-serif; font-size: 14px; color: #000000;\">{htmlMessage}</div>";

            return string.IsNullOrWhiteSpace(signatureHtml)
                ? messageDiv
                : $"{messageDiv}<br/>{FormatSignature(signatureHtml)}";
        }

        // La firma guardada en "Ajustes" puede ser HTML pegado (ej. exportado de Outlook,
        // empieza con una etiqueta) o texto plano sin formato. Si es HTML se inserta tal
        // cual; si es texto plano hay que encodearlo y convertir los saltos de linea a
        // <br/> para que no se rompa el layout del correo.
        private static string FormatSignature(string signature)
        {
            var trimmed = signature.Trim();
            if (trimmed.StartsWith('<'))
                return signature;

            var encoded = WebUtility.HtmlEncode(signature).Replace("\n", "<br/>");
            return $"<div style=\"font-family: Verdana, Geneva, sans-serif; font-size: 14px; color: #000000;\">{encoded}</div>";
        }
    }
}
