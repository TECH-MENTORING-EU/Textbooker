using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class AddItemFlaggedForReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FlaggedForReview",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlaggedForReview",
                table: "Items");
        }
    }
}
