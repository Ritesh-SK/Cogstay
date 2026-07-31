using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CogStayMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddStayRecordFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingReference",
                table: "StayRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BookingReference",
                table: "StayRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestName",
                table: "StayRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StayDetails",
                table: "StayRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingReference",
                table: "StayRecords");

            migrationBuilder.DropColumn(
                name: "BookingReference",
                table: "StayRecords");

            migrationBuilder.DropColumn(
                name: "GuestName",
                table: "StayRecords");

            migrationBuilder.DropColumn(
                name: "StayDetails",
                table: "StayRecords");
        }
    }
}
