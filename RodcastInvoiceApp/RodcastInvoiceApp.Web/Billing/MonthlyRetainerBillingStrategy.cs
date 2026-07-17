using System.Text.Json;
using RodcastInvoiceApp.Web.Data.Models;

namespace RodcastInvoiceApp.Web.Billing
{
    public class MonthlyRetainerBillingStrategy : IBillingStrategy
    {
        private static readonly string[] MonthNames =
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        };

        public BillingType BillingType => BillingType.MonthlyRetainer;

        public IReadOnlyList<InvoiceItemDraft> BuildInvoiceItems(Project project, BillingInput input, decimal vatPercent)
        {
            if (input is not MonthlyRetainerInput monthlyInput)
                throw new ArgumentException(
                    "Se esperaba MonthlyRetainerInput para un proyecto de retainer mensual.", nameof(input));

            var config = ParseConfig(project.Config);
            var monthLabel = $"{MonthNames[monthlyInput.Month - 1]} {monthlyInput.Year}";
            var items = new List<InvoiceItemDraft>();

            if (monthlyInput.VacationDays == 0)
            {
                // Mes normal: el monto es fijo (Subtotal + VAT = MonthlyRetainerAmount exacto,
                // sin importar el VatPercent de esta factura en particular).
                var subtotal = Math.Round(
                    config.MonthlyRetainerAmount / (1 + vatPercent / 100m), 2, MidpointRounding.AwayFromZero);
                var quantity = Math.Round(subtotal / config.HourlyRate, 2, MidpointRounding.AwayFromZero);

                items.Add(new InvoiceItemDraft
                {
                    Description = $"Engineer Onsite Support {monthLabel}",
                    Quantity = quantity,
                    Unit = "hours",
                    Rate = config.HourlyRate,
                    Amount = subtotal
                });
            }
            else
            {
                // Mes con vacaciones: se factura por horas realmente trabajadas, sin monto fijo.
                var quantity = monthlyInput.WorkedDays * 8m;
                var amount = Math.Round(quantity * config.HourlyRate, 2, MidpointRounding.AwayFromZero);

                items.Add(new InvoiceItemDraft
                {
                    Description = $"Engineer Onsite Support {monthLabel} " +
                                  $"({monthlyInput.WorkedDays} days worked, {monthlyInput.VacationDays} vacation days)",
                    Quantity = quantity,
                    Unit = "hours",
                    Rate = config.HourlyRate,
                    Amount = amount
                });
            }

            if (monthlyInput.OvertimeHoursToInvoice > 0)
            {
                // Horas extra del mes anterior, linea aparte, sin tope ni acumulacion.
                var overtimeRate = Math.Round(
                    config.HourlyRate * config.OvertimeMultiplier, 2, MidpointRounding.AwayFromZero);
                var overtimeAmount = Math.Round(
                    monthlyInput.OvertimeHoursToInvoice * overtimeRate, 2, MidpointRounding.AwayFromZero);

                items.Add(new InvoiceItemDraft
                {
                    Description = "Overtime (previous month)",
                    Quantity = monthlyInput.OvertimeHoursToInvoice,
                    Unit = "hours",
                    Rate = overtimeRate,
                    Amount = overtimeAmount
                });
            }

            return items;
        }

        private static MonthlyRetainerConfig ParseConfig(string configJson)
        {
            var config = JsonSerializer.Deserialize<MonthlyRetainerConfig>(
                configJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return config ?? throw new InvalidOperationException(
                "El Config del proyecto no es un JSON valido para retainer mensual.");
        }

        private class MonthlyRetainerConfig
        {
            public decimal HourlyRate { get; set; }
            public decimal MonthlyRetainerAmount { get; set; }
            public decimal OvertimeMultiplier { get; set; } = 1.5m;
        }
    }
}
