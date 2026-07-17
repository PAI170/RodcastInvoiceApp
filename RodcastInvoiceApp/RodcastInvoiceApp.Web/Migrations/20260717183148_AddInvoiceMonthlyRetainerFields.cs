using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RodcastInvoiceApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceMonthlyRetainerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeHoursToInvoice",
                table: "Invoices",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "VacationDays",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkedDays",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OvertimeHoursToInvoice",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "VacationDays",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "WorkedDays",
                table: "Invoices");
        }
    }
}
