using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RodcastInvoiceApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceSentByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SentByUserId",
                table: "Invoices",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SentByUserId",
                table: "Invoices",
                column: "SentByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_AspNetUsers_SentByUserId",
                table: "Invoices",
                column: "SentByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_AspNetUsers_SentByUserId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_SentByUserId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SentByUserId",
                table: "Invoices");
        }
    }
}
