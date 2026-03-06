using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarGarage.Data.Migrations
{
    /// <inheritdoc />
    public partial class UsersCarsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Vin = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: false),
                    Make = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModelYear = table.Column<int>(type: "int", nullable: false),
                    Trim = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FuelTypePrimary = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EngineCylinders = table.Column<int>(type: "int", nullable: true),
                    DisplacementL = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EngineHP = table.Column<int>(type: "int", nullable: true),
                    DriveType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransmissionStyle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransmissionSpeeds = table.Column<int>(type: "int", nullable: true),
                    BodyClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Doors = table.Column<int>(type: "int", nullable: true),
                    Seats = table.Column<int>(type: "int", nullable: true),
                    SeatRows = table.Column<int>(type: "int", nullable: true),
                    WheelbaseShort = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PlantCountry = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlantCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlantState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Mileage = table.Column<int>(type: "int", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCars",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CarId = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCars", x => new { x.UserId, x.CarId });
                    table.ForeignKey(
                        name: "FK_UserCars_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCars_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Vin",
                table: "Cars",
                column: "Vin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCars_CarId",
                table: "UserCars",
                column: "CarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCars");

            migrationBuilder.DropTable(
                name: "Cars");
        }
    }
}
