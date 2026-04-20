using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarGarage.Data.Migrations
{
    public partial class SyncModelWithDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Нищо не правим – базата вече е синхронизирана
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Нищо
        }
    }
}
