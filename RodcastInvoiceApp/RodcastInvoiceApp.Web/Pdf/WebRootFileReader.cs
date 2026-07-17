namespace RodcastInvoiceApp.Web.Pdf
{
    public static class WebRootFileReader
    {
        public static byte[]? TryRead(IWebHostEnvironment webHostEnvironment, string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return null;

            // Path.Combine ignora el primer argumento si el segundo empieza con "/" o "\"
            // (lo trata como ruta absoluta), asi que hay que quitar esos caracteres primero.
            var trimmedPath = relativePath.TrimStart('/', '\\');
            var fullPath = Path.Combine(webHostEnvironment.WebRootPath, trimmedPath);
            return File.Exists(fullPath) ? File.ReadAllBytes(fullPath) : null;
        }
    }
}
