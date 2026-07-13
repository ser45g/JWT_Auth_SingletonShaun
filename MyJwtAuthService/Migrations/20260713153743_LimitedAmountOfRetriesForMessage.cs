using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyJwtAuthService.Migrations
{
    /// <inheritdoc />
    public partial class LimitedAmountOfRetriesForMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_OccuredOnUtc_ProcessedOnUtc",
                table: "OutboxMessages");

            migrationBuilder.AddColumn<bool>(
                name: "IsUnprocessable",
                table: "OutboxMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RetryAttemptsCount",
                table: "OutboxMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_IsUnprocessable",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "IsUnprocessable" },
                filter: "\"ProcessedOnUtc\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_IsUnprocessable",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "IsUnprocessable",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "RetryAttemptsCount",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_OccuredOnUtc_ProcessedOnUtc",
                table: "OutboxMessages",
                columns: new[] { "OccuredOnUtc", "ProcessedOnUtc" },
                filter: "\"ProcessedOnUtc\" IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "Id", "Content", "Type" });
        }
    }
}
