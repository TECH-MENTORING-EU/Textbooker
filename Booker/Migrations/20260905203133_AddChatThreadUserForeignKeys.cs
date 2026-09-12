using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class AddChatThreadUserForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The columns carried no constraint until now, so rows pointing at
            // deleted accounts may exist; AddForeignKey would fail validating
            // them. Threads (and their messages) of gone accounts are junk and
            // are removed first - the same cleanup account deletion performs.
            migrationBuilder.Sql(
                "DELETE FROM ChatMessages WHERE DealId IN (SELECT ChannelId FROM ChatThreads WHERE UserAId NOT IN (SELECT Id FROM AspNetUsers) OR UserBId NOT IN (SELECT Id FROM AspNetUsers))");
            migrationBuilder.Sql(
                "DELETE FROM ChatThreads WHERE UserAId NOT IN (SELECT Id FROM AspNetUsers) OR UserBId NOT IN (SELECT Id FROM AspNetUsers)");

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreads_UserBId",
                table: "ChatThreads",
                column: "UserBId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreads_AspNetUsers_UserAId",
                table: "ChatThreads",
                column: "UserAId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreads_AspNetUsers_UserBId",
                table: "ChatThreads",
                column: "UserBId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreads_AspNetUsers_UserAId",
                table: "ChatThreads");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreads_AspNetUsers_UserBId",
                table: "ChatThreads");

            migrationBuilder.DropIndex(
                name: "IX_ChatThreads_UserBId",
                table: "ChatThreads");
        }
    }
}
