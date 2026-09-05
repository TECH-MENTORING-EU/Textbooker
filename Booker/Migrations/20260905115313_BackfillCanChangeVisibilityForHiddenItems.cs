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
            // Before this change the block path only set IsVisible, so offers
            // hidden by an admin block kept CanChangeVisibility = true (the CLR
            // initializer sent on INSERT). The reworked unlock re-publishes only
            // rows with the flag false and would therefore never match them; this
            // re-marks every hidden row that still claims the flag. Items already
            // carrying false (including rows that predate the flag, which got the
            // column's SQL default) are skipped, keeping the statement cheap when
            // the migration is replayed on a large table.
            migrationBuilder.Sql("UPDATE Items SET CanChangeVisibility = 0 WHERE IsVisible = 0 AND CanChangeVisibility <> 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
