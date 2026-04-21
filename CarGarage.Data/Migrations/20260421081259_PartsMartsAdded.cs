using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarGarage.Data.Migrations
{
    public partial class PartsMartsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Създаваме само новите таблици
            migrationBuilder.CreateTable(
                name: "PartCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CarId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parts_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Parts_PartCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PartCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 2. Наливаме данните
            migrationBuilder.InsertData(
                table: "PartCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Двигател и Периферия" }, { 2, "Спирачна система" },
                    { 3, "Ходова част и Окачване" }, { 4, "Кормилно управление" },
                    { 5, "Трансмисия и Съединител" }, { 6, "Филтри, Масла и Течности" },
                    { 7, "Електрическа система и Запалване" }, { 8, "Горивна система" },
                    { 9, "Охладителна система" }, { 10, "Климатична система" },
                    { 11, "Изпускателна система" }, { 12, "Каросерия и Остъкляване" },
                    { 13, "Светлини и Оптика" }, { 14, "Интериор и Оборудване" },
                    { 15, "Гуми и Джанти" }, { 16, "Аксесоари и Други" }
                });

            // 3. Индексите
            migrationBuilder.CreateIndex(
                name: "IX_Parts_CarId",
                table: "Parts",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_CategoryId",
                table: "Parts",
                column: "CategoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Parts");
            migrationBuilder.DropTable(name: "PartCategories");
        }
    }
}