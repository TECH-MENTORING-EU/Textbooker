using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class ItemViewCascadeDeleteUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemViews_AspNetUsers_UserId",
                table: "ItemViews");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemViews_AspNetUsers_UserId",
                table: "ItemViews",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemViews_AspNetUsers_UserId",
                table: "ItemViews");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemViews_AspNetUsers_UserId",
                table: "ItemViews",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
