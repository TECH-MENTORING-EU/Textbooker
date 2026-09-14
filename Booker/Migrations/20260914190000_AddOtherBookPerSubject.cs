using Booker.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booker.Migrations
{
    // Data-only migration, so there is no Designer file and the model snapshot is unchanged.
    // Gives every real subject its own "Inna" catalog row: Item stores only BookId, so a
    // listing for a book missing from the catalog keeps its subject only via such a row.
    // Ids are -1000 - SubjectId, clear of the positive ids the school import scripts sync.
    // The same idempotent block is appended to docs/SQL/import_*.sql for subjects added later.
    [DbContext(typeof(DataContext))]
    [Migration("20260914190000_AddOtherBookPerSubject")]
    public partial class AddOtherBookPerSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET IDENTITY_INSERT Books ON;

                INSERT INTO Books (Id, Title, SubjectId, LevelId)
                SELECT -1000 - s.Id, N'Inna', s.Id, -1
                FROM Subjects s
                WHERE s.Id > 0
                  AND NOT EXISTS (SELECT 1 FROM Books b WHERE b.Title = N'Inna' AND b.SubjectId = s.Id);

                SET IDENTITY_INSERT Books OFF;

                INSERT INTO BookGrades (BookId, GradeId)
                SELECT b.Id, g.Id
                FROM Books b
                CROSS JOIN Grades g
                WHERE b.Id <= -1000
                  AND b.Title = N'Inna'
                  AND NOT EXISTS (SELECT 1 FROM BookGrades bg WHERE bg.BookId = b.Id AND bg.GradeId = g.Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM BookGrades WHERE BookId <= -1000;
                DELETE FROM Books WHERE Id <= -1000 AND Title = N'Inna';
                """);
        }
    }
}
