using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RodcastInvoiceApp.Web.DataTransferObjects.CompanySettings;
using RodcastInvoiceApp.Web.DataTransferObjects.Invoice;

namespace RodcastInvoiceApp.Web.Pdf
{
    // Replica el diseno de la plantilla historica de Rodcast (factura #011):
    // barra azul arriba/abajo, logo, dos columnas de datos, tabla de items,
    // totales alineados a la derecha, y pie con banco/firma.
    public class InvoicePdfDocument : IDocument
    {
        private const string PrimaryColor = "#1B3E7D";
        private const int BarHeight = 28;

        private static readonly string[] MonthNames =
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        };

        private readonly InvoiceResponseDto _invoice;
        private readonly CompanySettingsDto _company;
        private readonly byte[]? _logoBytes;
        private readonly byte[]? _signatureBytes;

        public InvoicePdfDocument(
            InvoiceResponseDto invoice,
            CompanySettingsDto company,
            byte[]? logoBytes,
            byte[]? signatureBytes)
        {
            _invoice = invoice;
            _company = company;
            _logoBytes = logoBytes;
            _signatureBytes = signatureBytes;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Height(BarHeight).Background(PrimaryColor);

                column.Item().PaddingHorizontal(30).PaddingTop(15).PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem().Height(110).AlignLeft().Element(logo =>
                    {
                        if (_logoBytes is not null)
                            logo.Image(_logoBytes).FitArea();
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().AlignRight().Text("INVOICE").FontSize(24).Bold().FontColor(PrimaryColor);
                        col.Item().AlignRight().Text($"No: {_invoice.InvoiceNumber}");
                    });
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingHorizontal(30).PaddingTop(5).Column(column =>
            {
                column.Spacing(10);

                // Bill To (izquierda) / Proyecto (derecha)
                column.Item().Row(row =>
                {
                    row.RelativeItem(6).Column(col =>
                    {
                        col.Item().Text("Bill To:").FontColor(PrimaryColor).FontSize(13);
                        col.Item().Text(text =>
                        {
                            text.Span("Company Name: ").Bold();
                            text.Span(_invoice.ClientName);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Address: ").Bold();
                            text.Span(_invoice.ClientAddress);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("VAT ID: ").Bold();
                            text.Span(_invoice.ClientVatId);
                        });
                    });

                    row.RelativeItem(4).Column(col =>
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Project: ").Bold();
                            text.Span(_invoice.ProjectName);
                        });
                        if (!string.IsNullOrWhiteSpace(_invoice.ProjectCostCenter))
                        {
                            col.Item().Text(text =>
                            {
                                text.Span("Cost Center: ").Bold();
                                text.Span(_invoice.ProjectCostCenter ?? string.Empty);
                            });
                        }
                        col.Item().Text(text =>
                        {
                            text.Span("Invoice Date: ").Bold();
                            text.Span(FormatDate(_invoice.InvoiceDate));
                        });
                        if (!string.IsNullOrWhiteSpace(_invoice.City))
                        {
                            col.Item().Text(text =>
                            {
                                text.Span("Ticket: ").Bold();
                                text.Span($"{_invoice.TicketNumber} - {_invoice.City} / {_invoice.SlaType}");
                            });
                        }
                    });
                });

                // Company Information (izquierda) / TAX Information (derecha)
                column.Item().Row(row =>
                {
                    row.RelativeItem(6).Column(col =>
                    {
                        col.Item().Text("Company Information").FontColor(PrimaryColor).FontSize(13);
                        col.Item().Text(text =>
                        {
                            text.Span("Company Name: ").Bold();
                            text.Span(_company.Name);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Address: ").Bold();
                            text.Span(_company.Address);
                        });
                        if (!string.IsNullOrWhiteSpace(_invoice.ClientSupplierIdAssigned))
                        {
                            col.Item().Text(text =>
                            {
                                text.Span("Supplier ID: ").Bold();
                                text.Span(_invoice.ClientSupplierIdAssigned ?? string.Empty);
                            });
                        }
                    });

                    row.RelativeItem(4).Column(col =>
                    {
                        col.Item().Text("TAX Information").FontColor(PrimaryColor).FontSize(13);
                        col.Item().Text(text =>
                        {
                            text.Span("TAX ID: ").Bold();
                            text.Span(_company.TaxId);
                        });
                    });
                });

                // Tabla de items
                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderTop(1).BorderBottom(1).BorderColor(PrimaryColor)
                            .PaddingVertical(4).Text("Item Description").FontColor(PrimaryColor);
                        header.Cell().BorderTop(1).BorderBottom(1).BorderColor(PrimaryColor)
                            .PaddingVertical(4).Text("Quantity").FontColor(PrimaryColor);
                        header.Cell().BorderTop(1).BorderBottom(1).BorderColor(PrimaryColor)
                            .PaddingVertical(4).Text("Rate").FontColor(PrimaryColor);
                        header.Cell().BorderTop(1).BorderBottom(1).BorderColor(PrimaryColor)
                            .PaddingVertical(4).Text("Total").FontColor(PrimaryColor);
                    });

                    foreach (var item in _invoice.Items)
                    {
                        table.Cell().PaddingVertical(5).Text(item.Description);
                        table.Cell().PaddingVertical(5).Text($"{item.Quantity:0.##} {item.Unit}");
                        table.Cell().PaddingVertical(5).Text($"${item.Rate:0.00}");
                        table.Cell().PaddingVertical(5).Text($"${item.Amount:0.00}");
                    }
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Column(outer =>
            {
                outer.Item().PaddingHorizontal(30).PaddingBottom(10).Column(column =>
                {
                    column.Spacing(6);

                    // Totales, alineados a la derecha (etiqueta y monto en un solo texto para que queden pegados)
                    column.Item().Row(row =>
                    {
                        row.RelativeItem(6);
                        row.RelativeItem(4).Column(col =>
                        {
                            col.Item().AlignRight().Text(text =>
                            {
                                text.Span("Subtotal: ");
                                text.Span($"${_invoice.Subtotal:0.00}");
                            });
                            col.Item().AlignRight().Text(text =>
                            {
                                var vatLabel = _invoice.IsVatExonerated
                                    ? $"VAT ({_invoice.VatPercent:0.##}% - exonerated): "
                                    : $"VAT ({_invoice.VatPercent:0.##}%): ";
                                text.Span(vatLabel);
                                text.Span($"${_invoice.VatAmount:0.00}");
                            });
                        });
                    });

                    column.Item()
                        .BorderTop(1).BorderBottom(1).BorderColor(PrimaryColor)
                        .PaddingVertical(5)
                        .Row(row =>
                        {
                            row.RelativeItem().Text($"Currency: {_invoice.Currency}");
                            row.RelativeItem().AlignRight()
                                .Text($"Total Amount Due: ${_invoice.Total:0.00}").Bold().FontSize(13);
                        });

                    // Terminos de pago / Cuenta bancaria (izquierda) y firma (derecha)
                    column.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem(6).Column(col =>
                        {
                            col.Item().Text("Payment Terms").Bold();
                            col.Item().Text($"Payment Method: {_invoice.PaymentMethod}");

                            col.Item().PaddingTop(6).Text("Bank Account Details").Bold();
                            col.Item().Text($"Bank Name: {_invoice.BankAccountName}");
                            col.Item().Text(text =>
                            {
                                text.Span("SWIFT CODE: ").Bold();
                                text.Span(_invoice.BankAccountSwift ?? string.Empty);
                            });
                            col.Item().Text(text =>
                            {
                                text.Span("IBAN: ").Bold();
                                text.Span(_invoice.BankAccountIban ?? string.Empty);
                            });
                        });

                        row.RelativeItem(4).Column(col =>
                        {
                            col.Item().AlignRight().Height(35).Element(sign =>
                            {
                                if (_signatureBytes is not null)
                                    sign.Image(_signatureBytes).FitArea();
                            });
                            col.Item().AlignRight().Width(120)
                                .BorderBottom(1).BorderColor(Colors.Black).Height(4);
                            col.Item().AlignRight().Text("Sign").FontColor(PrimaryColor);
                            col.Item().PaddingTop(8).AlignRight().Text(text =>
                            {
                                text.Span("Due for payment: ").Bold();
                                text.Span(FormatDate(_invoice.DueDate)).Bold();
                            });
                        });
                    });
                });

                // Barra a todo el ancho, fuera del padding lateral (igual que el header).
                outer.Item().Height(BarHeight).Background(PrimaryColor);
            });
        }

        private static string FormatDate(DateTime date) =>
            $"{date.Day} {MonthNames[date.Month - 1]} {date.Year}";
    }
}
