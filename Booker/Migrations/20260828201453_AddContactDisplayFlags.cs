using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class AddContactDisplayFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DisplayInstagram",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DisplayMessenger",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayInstagram",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DisplayMessenger",
                table: "AspNetUsers");
        }
    }
}
