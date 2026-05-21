using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booker.Migrations
{
    public partial class AddRatingReplyFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reply",
                table: "UserRatings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepliedAt",
                table: "UserRatings",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reply",
                table: "UserRatings");

            migrationBuilder.DropColumn(
                name: "RepliedAt",
                table: "UserRatings");
        }
    }
}
