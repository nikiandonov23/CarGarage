using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarGarage.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageStatusTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "Messages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SeenAt",
                table: "Messages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSent",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "SeenAt",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsSent",
                table: "Messages");
        }
    }
}
