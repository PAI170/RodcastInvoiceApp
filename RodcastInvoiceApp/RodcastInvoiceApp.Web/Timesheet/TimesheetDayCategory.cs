namespace RodcastInvoiceApp.Web.Timesheet
{
    // No incluye "Present": ese es el valor por defecto de cualquier dia entre
    // semana del mes que no tenga una excepcion guardada.
    public enum TimesheetDayCategory
    {
        PlannedLeave,
        PublicHoliday,
        UnplannedLeave,
        Backfill,
        SickLeave,
        SupportOnPublicHoliday
    }
}
