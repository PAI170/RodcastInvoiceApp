namespace RodcastInvoiceApp.Web.Timesheet
{
    // Colores y etiquetas compartidos entre la pagina Blazor y el PDF,
    // para que ambos se vean exactamente igual.
    public static class TimesheetColors
    {
        public const string Present = "#C6F76F";
        public const string PlannedLeave = "#F2994A";
        public const string PublicHoliday = "#F2C94C";
        public const string UnplannedLeave = "#EB5757";
        public const string Backfill = "#6FCFF0";
        public const string SickLeave = "#2F80ED";
        public const string SupportOnPublicHoliday = "#F06BA8";
        public const string Empty = "#2E2E2E";

        public static string GetHex(TimesheetDayCategory? category) => category switch
        {
            null => Present,
            TimesheetDayCategory.PlannedLeave => PlannedLeave,
            TimesheetDayCategory.PublicHoliday => PublicHoliday,
            TimesheetDayCategory.UnplannedLeave => UnplannedLeave,
            TimesheetDayCategory.Backfill => Backfill,
            TimesheetDayCategory.SickLeave => SickLeave,
            TimesheetDayCategory.SupportOnPublicHoliday => SupportOnPublicHoliday,
            _ => Present
        };

        public static string GetLabel(TimesheetDayCategory? category) => category switch
        {
            null => "Present",
            TimesheetDayCategory.PlannedLeave => "Planned Leave",
            TimesheetDayCategory.PublicHoliday => "Public Holiday",
            TimesheetDayCategory.UnplannedLeave => "Unplanned Leave",
            TimesheetDayCategory.Backfill => "Backfill",
            TimesheetDayCategory.SickLeave => "Sick Leave",
            TimesheetDayCategory.SupportOnPublicHoliday => "Support on Public Holiday",
            _ => "Present"
        };
    }
}
