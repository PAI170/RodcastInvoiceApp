using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RodcastInvoiceApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUserImapPort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImapPort",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImapPort",
                table: "AspNetUsers");
        }
    }
}
