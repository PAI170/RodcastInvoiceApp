using System.Text.Json;
using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.Exceptions;

namespace RodcastInvoiceApp.Web.Billing
{
    public class PerTicketBillingStrategy : IBillingStrategy
    {
        public BillingType BillingType => BillingType.PerTicket;

        public IReadOnlyList<InvoiceItemDraft> BuildInvoiceItems(Project project, BillingInput input, decimal vatPercent)
        {
            if (input is not PerTicketInput ticketInput)
                throw new ArgumentException(
                    "Se esperaba PerTicketInput para un proyecto de soporte por ticket.", nameof(input));

            var baseRule = project.PriceRules.FirstOrDefault(pr =>
                pr.Dimension1 == ticketInput.City && pr.Dimension2 == ticketInput.SlaType)
                ?? throw new BadRequestException(
                    $"No hay una tarifa configurada para '{ticketInput.City}' / '{ticketInput.SlaType}' en este proyecto.");

            var config = ParseConfig(project.Config);
            var items = new List<InvoiceItemDraft>
            {
                new()
                {
                    Description = $"Ticket {ticketInput.TicketNumber} - {ticketInput.City} - {ticketInput.SlaType} " +
                                  "(incluye traslado y primera hora)",
                    Quantity = 1,
                    Unit = "ticket",
                    Rate = baseRule.Rate,
                    Amount = baseRule.Rate
                }
            };

            if (ticketInput.ApprovedAdditionalMinutes > 0)
            {
                // El tiempo adicional se factura en incrementos de 15 min: se redondea
                // hacia arriba al siguiente incremento completo (ej. 20 min -> 30 min).
                var incrementMinutes = config.AdditionalTimeIncrementMinutes;
                var incrementsCount = Math.Ceiling(
                    ticketInput.ApprovedAdditionalMinutes / (double)incrementMinutes);
                var billedMinutes = (decimal)(incrementsCount * incrementMinutes);
                var billedHours = billedMinutes / 60m;
                var amount = Math.Round(billedHours * config.AdditionalHourRate, 2, MidpointRounding.AwayFromZero);

                items.Add(new InvoiceItemDraft
                {
                    Description = $"Tiempo adicional aprobado ({billedMinutes:0} min)",
                    Quantity = billedHours,
                    Unit = "hours",
                    Rate = config.AdditionalHourRate,
                    Amount = amount
                });
            }

            if (ticketInput.OvertimeHours > 0)
            {
                var overtimeRate = Math.Round(
                    config.AdditionalHourRate * config.OvertimeMultiplier, 2, MidpointRounding.AwayFromZero);
                var overtimeAmount = Math.Round(
                    ticketInput.OvertimeHours * overtimeRate, 2, MidpointRounding.AwayFromZero);

                items.Add(new InvoiceItemDraft
                {
                    Description = "Horas extra",
                    Quantity = ticketInput.OvertimeHours,
                    Unit = "hours",
                    Rate = overtimeRate,
                    Amount = overtimeAmount
                });
            }

            return items;
        }

        private static PerTicketConfig ParseConfig(string configJson)
        {
            var config = JsonSerializer.Deserialize<PerTicketConfig>(
                configJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return config ?? throw new InvalidOperationException(
                "El Config del proyecto no es un JSON valido para soporte por ticket.");
        }

        private class PerTicketConfig
        {
            public decimal AdditionalHourRate { get; set; }
            public int AdditionalTimeIncrementMinutes { get; set; } = 15;
            public decimal OvertimeMultiplier { get; set; } = 1.5m;
        }
    }
}
