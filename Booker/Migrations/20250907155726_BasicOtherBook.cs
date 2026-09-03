using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class BasicOtherBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Subjects",
                columns: new[] { "Id", "Name" },
                values: new object[] { -1, "Brak" });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "LevelId", "SubjectId", "Title" },
                values: new object[] { -1, -1, -1, "Inna" });

            migrationBuilder.InsertData(
                table: "BookGrades",
                columns: new[] { "BookId", "GradeId" },
                values: new object[,]
                {
                    { -1, 1 },
                    { -1, 2 },
                    { -1, 3 },
                    { -1, 4 },
                    { -1, 5 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { -1, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { -1, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { -1, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { -1, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { -1, 5 });

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: -1);
        }
    }
}
