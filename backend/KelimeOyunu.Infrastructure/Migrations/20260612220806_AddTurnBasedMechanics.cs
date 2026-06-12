using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KelimeOyunu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTurnBasedMechanics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActiveTurnPlayerId",
                table: "GameRounds",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveTurnPlayerId",
                table: "GameRounds");
        }
    }
}
