using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class BackfillReservedAtForLegacyReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rows reserved before the transaction lifecycle shipped carry
            // Reserved = true but no ReservedAt stamp, so the 30-day auto
            // release skips them forever. Start their window at deploy time:
            // after 30 more days an unconfirmed reservation is released.
            migrationBuilder.Sql(
                "UPDATE Items SET ReservedAt = SYSDATETIME() " +
                "WHERE Reserved = CONVERT(bit, 1) AND IsSold = CONVERT(bit, 0) " +
                "AND ReservedAt IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data backfill is not reversible; the pre-deploy nulls cannot be
            // told apart from post-deploy reservations.
        }
    }
}
