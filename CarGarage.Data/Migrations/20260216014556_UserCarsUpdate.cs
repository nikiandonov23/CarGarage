using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarGarage.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserCarsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "UserCars");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "UserCars");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "UserCars",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "UserCars",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
