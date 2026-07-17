using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RodcastInvoiceApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceTimesheetExceptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimesheetExceptions",
                table: "Invoices",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimesheetExceptions",
                table: "Invoices");
        }
    }
}
