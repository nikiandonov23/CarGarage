using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarGarage.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClearAllMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete all messages from the Messages table
            migrationBuilder.Sql("DELETE FROM [Messages]");

            // Reset identity seed to 1
            migrationBuilder.Sql("DBCC CHECKIDENT('[Messages]', RESEED, 0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No need to restore messages on rollback
        }
    }
}
