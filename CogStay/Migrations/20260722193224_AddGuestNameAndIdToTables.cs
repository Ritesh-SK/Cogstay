using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CogStayMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestNameAndIdToTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuestName",
                table: "Reservations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GuestId",
                table: "Billings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GuestName",
                table: "Billings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestName",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "GuestId",
                table: "Billings");

            migrationBuilder.DropColumn(
                name: "GuestName",
                table: "Billings");
        }
    }
}
