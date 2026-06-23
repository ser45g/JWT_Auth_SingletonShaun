using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyJwtAuthService.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_OccuredOnUtc_ProcessedOnUtc",
                table: "OutboxMessages",
                columns: new[] { "OccuredOnUtc", "ProcessedOnUtc" },
                filter: "\"ProcessedOnUtc\" IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "Id", "Content", "Type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_OccuredOnUtc_ProcessedOnUtc",
                table: "OutboxMessages");
        }
    }
}
