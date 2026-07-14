using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Webhooks.Processing.Migrations
{
    /// <inheritdoc />
    public partial class MadeWebhookUrlEventTypeCombinationUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WebhookUrl",
                schema: "webhooks",
                table: "webhook_subscriptions",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_subscriptions_EventType_WebhookUrl",
                schema: "webhooks",
                table: "webhook_subscriptions",
                columns: new[] { "EventType", "WebhookUrl" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_webhook_subscriptions_EventType_WebhookUrl",
                schema: "webhooks",
                table: "webhook_subscriptions");

            migrationBuilder.AlterColumn<string>(
                name: "WebhookUrl",
                schema: "webhooks",
                table: "webhook_subscriptions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024);
        }
    }
}
