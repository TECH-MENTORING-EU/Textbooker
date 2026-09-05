using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class BackfillCanChangeVisibilityForHiddenItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Items hidden before CanChangeVisibility existed were all hidden by an
            // admin block (no user-side hide ships yet), but carry the column default
            // of true. Without this backfill the fixed unlock query would never match
            // them and those users could never be re-published. The extra predicate
            // skips rows that are already backfilled, keeping the statement cheap
            // when the migration is replayed on a large table.
            migrationBuilder.Sql("UPDATE Items SET CanChangeVisibility = 0 WHERE IsVisible = 0 AND CanChangeVisibility <> 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
