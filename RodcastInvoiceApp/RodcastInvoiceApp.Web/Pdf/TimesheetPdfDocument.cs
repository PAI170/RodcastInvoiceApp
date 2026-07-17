using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RodcastInvoiceApp.Web.DataTransferObjects.Timesheet;
using RodcastInvoiceApp.Web.Timesheet;

namespace RodcastInvoiceApp.Web.Pdf
{
    // Replica el diseno del timesheet mensual que se envia junto con la factura:
    // fondo oscuro, calendario del mes coloreado por categoria, y una leyenda.
    public class TimesheetPdfDocument : IDocument
    {
        private const string BackgroundColor = "#141414";
        private const string CellEmptyColor = "#2A2A2A";
        private const string TextLight = "#FFFFFF";
        private const string TextDim = "#8A8A8A";
        private const string TextDark = "#1A1A1A";

        private static readonly string[] MonthNames =
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        };

        private static readonly string[] DayHeaders = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        private readonly TimesheetDto _timesheet;
        private readonly byte[]? _logoBytes;

        public TimesheetPdfDocument(TimesheetDto timesheet, byte[]? logoBytes)
        {
            _timesheet = timesheet;
            _logoBytes = logoBytes;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.PageColor(BackgroundColor);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(TextLight));

                page.Content().Column(column =>
                {
                    column.Spacing(20);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().AlignMiddle().Text(MonthNames[_timesheet.Month - 1]).FontSize(40).Bold();

                        row.RelativeItem().Row(inner =>
                        {
                            inner.RelativeItem().AlignMiddle().AlignRight()
                                .Text(_timesheet.ProjectName.ToUpperInvariant()).FontSize(12);

                            if (_logoBytes is not null)
                            {
                                inner.ConstantItem(70).Height(55).PaddingLeft(12).AlignMiddle().Element(logo =>
                                    logo.Image(_logoBytes).FitArea());
                            }
                        });
                    });

                    column.Item().Row(row =>
                    {
                        row.RelativeItem(7).Element(ComposeCalendar);
                        row.RelativeItem(3).PaddingLeft(25).Element(ComposeLegend);
                    });
                });
            });
        }

        private void ComposeCalendar(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(6);

                column.Item().Row(row =>
                {
                    foreach (var header in DayHeaders)
                        row.RelativeItem().AlignCenter().Text(header);
                });

                foreach (var week in _timesheet.Weeks)
                {
                    column.Item().Row(row =>
                    {
                        foreach (var cell in week)
                        {
                            var background = cell.IsColorable ? TimesheetColors.GetHex(cell.Category) : CellEmptyColor;
                            var textColor = cell.IsColorable ? TextDark : TextDim;

                            row.RelativeItem().AspectRatio(1.2f).Padding(3).Background(background).CornerRadius(8)
                                .Padding(6).AlignTop().Text(cell.Day.ToString()).FontColor(textColor);
                        }
                    });
                }
            });
        }

        private void ComposeLegend(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(12);

                column.Item().Row(row =>
                {
                    row.Spacing(20);
                    ComposeLegendItem(row.RelativeItem(), null);
                    ComposeLegendItem(row.RelativeItem(), TimesheetDayCategory.PlannedLeave);
                });
                column.Item().Row(row =>
                {
                    row.Spacing(20);
                    ComposeLegendItem(row.RelativeItem(), TimesheetDayCategory.UnplannedLeave);
                    ComposeLegendItem(row.RelativeItem(), TimesheetDayCategory.SickLeave);
                });
                column.Item().Row(row =>
                {
                    row.Spacing(20);
                    ComposeLegendItem(row.RelativeItem(), TimesheetDayCategory.PublicHoliday);
                    ComposeLegendItem(row.RelativeItem(), TimesheetDayCategory.Backfill);
                });
                column.Item().Row(row =>
                {
                    ComposeLegendItem(row.RelativeItem(), TimesheetDayCategory.SupportOnPublicHoliday);
                });
            });
        }

        private static void ComposeLegendItem(IContainer container, TimesheetDayCategory? category)
        {
            container.Row(row =>
            {
                row.ConstantItem(14).Height(14).Background(TimesheetColors.GetHex(category));
                row.RelativeItem().PaddingLeft(6).Text(TimesheetColors.GetLabel(category).ToUpperInvariant()).FontSize(9);
            });
        }
    }
}
