using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelimeOyunu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTokensToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastTokenUpdateTime",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Tokens",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastTokenUpdateTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Tokens",
                table: "Users");
        }
    }
}
