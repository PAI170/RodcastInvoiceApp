namespace RodcastInvoiceApp.Web.Timesheet
{
    // Logica compartida para armar la cuadricula del mes (semanas x 7 dias),
    // usada tanto por la pagina Blazor interactiva como por el PDF.
    public static class TimesheetCalendarBuilder
    {
        public static List<List<TimesheetDayCell>> BuildWeeks(
            int year, int month, IReadOnlyList<TimesheetDayException> exceptions)
        {
            var exceptionsByDay = exceptions.ToDictionary(e => e.Day, e => e.Category);

            var firstOfMonth = new DateTime(year, month, 1);
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var gridStart = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);

            // Redondea hacia arriba a semanas completas (5 o 6) que cubran todo el mes.
            var totalCells = (int)Math.Ceiling((daysInMonth + (int)firstOfMonth.DayOfWeek) / 7.0) * 7;

            var weeks = new List<List<TimesheetDayCell>>();
            var current = gridStart;

            for (var cellIndex = 0; cellIndex < totalCells; cellIndex++)
            {
                if (cellIndex % 7 == 0)
                    weeks.Add(new List<TimesheetDayCell>());

                var isCurrentMonth = current.Month == month && current.Year == year;
                var isWeekend = current.DayOfWeek == DayOfWeek.Saturday || current.DayOfWeek == DayOfWeek.Sunday;

                TimesheetDayCategory? category = null;
                if (isCurrentMonth && !isWeekend && exceptionsByDay.TryGetValue(current.Day, out var foundCategory))
                    category = foundCategory;

                weeks[^1].Add(new TimesheetDayCell
                {
                    Day = current.Day,
                    IsCurrentMonth = isCurrentMonth,
                    IsWeekend = isWeekend,
                    Category = category
                });

                current = current.AddDays(1);
            }

            return weeks;
        }
    }
}
