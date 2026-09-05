using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class AddChatThreadReadStamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UserAReadUtc",
                table: "ChatThreads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UserBReadUtc",
                table: "ChatThreads",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserAReadUtc",
                table: "ChatThreads");

            migrationBuilder.DropColumn(
                name: "UserBReadUtc",
                table: "ChatThreads");
        }
    }
}
