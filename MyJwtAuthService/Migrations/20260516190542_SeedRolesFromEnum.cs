using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyJwtAuthService.Migrations
{
    /// <inheritdoc />
    public partial class SeedRolesFromEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("600eb033-f583-460b-aece-4414b1106a78"), "c04ff1b8-711d-4cfa-bc4d-8424f768ab21", "User", null },
                    { new Guid("6aff0ed0-ab58-4795-81aa-81c81e549123"), "ee4268a6-7cd7-4132-ba51-8c5913f8ce70", "Admin", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("600eb033-f583-460b-aece-4414b1106a78"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("6aff0ed0-ab58-4795-81aa-81c81e549123"));
        }
    }
}
