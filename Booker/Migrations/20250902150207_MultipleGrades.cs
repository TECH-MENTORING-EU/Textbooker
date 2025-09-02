using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class MultipleGrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 11, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 34, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 36, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 38, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 40, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 41, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 42, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 43, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 44, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 45, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 46, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 47, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 49, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 51, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 52, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 53, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 54, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 55, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 56, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 57, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 59, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 61, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 65, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 69, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 70, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 71, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 72, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 73, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 74, 5 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 75, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 76, 3 });

            migrationBuilder.InsertData(
                table: "Subjects",
                columns: new[] { "Id", "Name" },
                values: new object[] { 18, "Edukacja obywatelska" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { true, 4, "Biologia na czasie 4" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 35,
                column: "Title",
                value: "To jest chemia 1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Level", "Title" },
                values: new object[] { false, "To jest chemia 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 37,
                column: "Title",
                value: "To jest chemia 1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { true, 5, "To jest chemia 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { false, 6, "Edukacja dla bezpieczeństwa [wsip]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 40,
                column: "Title",
                value: "Fizyka 1 [wsip]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 41,
                column: "Title",
                value: "Fizyka 2 [wsip]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 42,
                column: "Title",
                value: "Fizyka 3 [wsip]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Level", "Title" },
                values: new object[] { true, "Fizyka 4 [wsip]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 44,
                column: "Title",
                value: "Fizyka 1 [wsip]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 45,
                column: "Title",
                value: "Fizyka 2 [wsip]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 46,
                column: "Title",
                value: "Fizyka 3 [wsip]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 7, "Fizyka 4 [wsip]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 48,
                column: "Title",
                value: "Oblicza geografii 1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 49,
                column: "Title",
                value: "Oblicza geografii 2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 50,
                column: "Title",
                value: "Oblicza geografii karty pracy 1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 8, "Oblicza geografii karty pracy 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 52,
                column: "Title",
                value: "Historia [wsip] 1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 53,
                column: "Title",
                value: "Historia [wsip] 2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 54,
                column: "Title",
                value: "Historia [wsip] 3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 9, "Historia [wsip] 4" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 56,
                column: "Title",
                value: "Historia i teraźniejszość [wsip] 1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 10, "Historia i teraźniejszość [wsip] 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 58,
                column: "Title",
                value: "Informatyka [operon]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 59,
                columns: new[] { "Level", "Title" },
                values: new object[] { false, "Informatyka dla szkół ponadgimnazjalnych [Migra]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 60,
                column: "Title",
                value: "Informatyka [operon]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 61,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { true, 11, "Informatyka dla szkół ponadgimnazjalnych [Migra]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 62,
                column: "Title",
                value: "NOWA MATeMAtyka 1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 63,
                column: "Title",
                value: "NOWA MATeMAtyka 2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 64,
                column: "Title",
                value: "NOWA MATeMAtyka 3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 65,
                columns: new[] { "Level", "Title" },
                values: new object[] { false, "NOWA MATeMAtyka 4" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 66,
                column: "Title",
                value: "NOWA MATeMAtyka 1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 67,
                column: "Title",
                value: "NOWA MATeMAtyka 2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 68,
                column: "Title",
                value: "NOWA MATeMAtyka 3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 69,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { true, 12, "NOWA MATeMAtyka 4" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 70,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 13, "Krok w przedsiębiorczość" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 71,
                column: "Title",
                value: "Krok w biznes i zarządzanie 1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 72,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 14, "Krok w biznes i zarządzanie 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 73,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 15, "Spotkania ze sztuką 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 74,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 18, "Masz wpływ 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 75,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { false, 16, "W centrum uwagi 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 76,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { false, 16, "W centrum uwagi 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 77,
                column: "Title",
                value: "Electronics");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 78,
                columns: new[] { "Level", "Title" },
                values: new object[] { true, "Electrician" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 79,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 17, "Software engineering" });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Level", "SubjectId", "Title" },
                values: new object[,]
                {
                    { 80, true, 17, "Computing" },
                    { 81, true, 17, "Mechanical engineering" },
                    { 82, true, 17, "Mechanics" },
                    { 83, true, 17, "Environmental Science" },
                    { 84, false, 17, "IT [english for IT]" },
                    { 85, true, 11, "Informatyka w praktyce" }
                });
            
            migrationBuilder.InsertData(
                table: "BookGrades",
                columns: new[] { "BookId", "GradeId" },
                values: new object[,]
                {
                    { 4, 3 },
                    { 6, 3 },
                    { 7, 4 },
                    { 8, 2 },
                    { 8, 3 },
                    { 9, 2 },
                    { 9, 3 },
                    { 10, 1 },
                    { 10, 3 },
                    { 10, 4 },
                    { 11, 3 },
                    { 11, 4 },
                    { 11, 5 },
                    { 12, 1 },
                    { 12, 2 },
                    { 13, 1 },
                    { 13, 2 },
                    { 14, 1 },
                    { 14, 2 },
                    { 14, 3 },
                    { 15, 3 },
                    { 15, 5 },
                    { 16, 4 },
                    { 21, 1 },
                    { 23, 5 },
                    { 24, 2 },
                    { 25, 3 },
                    { 26, 4 },
                    { 27, 5 },
                    { 29, 3 },
                    { 30, 4 },
                    { 34, 4 },
                    { 35, 1 },
                    { 35, 3 },
                    { 36, 2 },
                    { 36, 3 },
                    { 36, 4 },
                    { 37, 1 },
                    { 37, 3 },
                    { 38, 2 },
                    { 38, 3 },
                    { 38, 4 },
                    { 38, 5 },
                    { 40, 1 },
                    { 41, 2 },
                    { 42, 3 },
                    { 43, 4 },
                    { 43, 5 },
                    { 44, 1 },
                    { 45, 2 },
                    { 46, 3 },
                    { 47, 4 },
                    { 47, 5 },
                    { 48, 1 },
                    { 49, 2 },
                    { 49, 3 },
                    { 49, 4 },
                    { 50, 1 },
                    { 51, 2 },
                    { 51, 3 },
                    { 51, 4 },
                    { 52, 1 },
                    { 53, 2 },
                    { 54, 3 },
                    { 55, 4 },
                    { 55, 5 },
                    { 56, 2 },
                    { 57, 3 },
                    { 58, 1 },
                    { 59, 2 },
                    { 59, 3 },
                    { 59, 4 },
                    { 60, 1 },
                    { 61, 2 },
                    { 61, 3 },
                    { 61, 4 },
                    { 62, 1 },
                    { 63, 2 },
                    { 64, 3 },
                    { 65, 4 },
                    { 65, 5 },
                    { 66, 1 },
                    { 67, 2 },
                    { 68, 3 },
                    { 69, 4 },
                    { 69, 5 },
                    { 70, 2 },
                    { 71, 1 },
                    { 72, 2 },
                    { 73, 1 },
                    { 74, 1 },
                    { 74, 2 },
                    { 75, 4 },
                    { 75, 5 },
                    { 76, 4 },
                    { 76, 5 },
                    { 77, 4 },
                    { 78, 4 },
                    { 79, 4 }
                });

            migrationBuilder.InsertData(
                table: "BookGrades",
                columns: new[] { "BookId", "GradeId" },
                values: new object[,]
                {
                    { 80, 3 },
                    { 80, 4 },
                    { 81, 3 },
                    { 81, 4 },
                    { 82, 3 },
                    { 82, 4 },
                    { 83, 3 },
                    { 83, 4 },
                    { 84, 3 },
                    { 84, 4 },
                    { 85, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 4, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 6, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 7, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 8, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 8, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 9, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 9, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 10, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 10, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 10, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 11, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 11, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 11, 5 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 12, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 12, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 13, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 13, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 14, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 14, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 14, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 15, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 15, 5 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 16, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 21, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 23, 5 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 24, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 25, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 26, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 27, 5 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 29, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 30, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 34, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 35, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 35, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 36, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 36, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 36, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 37, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 37, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 38, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 38, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 38, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 38, 5 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 40, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 41, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 42, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 43, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 43, 5 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 44, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 45, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 46, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 47, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 47, 5 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 48, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 49, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 49, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 49, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 50, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 51, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 51, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 51, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 52, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 53, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 54, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 55, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 55, 5 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 56, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 57, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 58, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 59, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 59, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 59, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 60, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 61, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 61, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 61, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 62, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 63, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 64, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 65, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 65, 5 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 66, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 67, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 68, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 69, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 69, 5 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 70, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 71, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 72, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 73, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 74, 1 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 74, 2 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 75, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 75, 5 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 76, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 76, 5 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 77, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 78, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 79, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 80, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 80, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 81, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 81, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 82, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 82, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 83, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 83, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 84, 3 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 84, 4 });

            migrationBuilder.DeleteData(
                table: "BookGrades",
                keyColumns: new[] { "BookId", "GradeId" },
                keyValues: new object[] { 85, 3 });

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.InsertData(
                table: "BookGrades",
                columns: new[] { "BookId", "GradeId" },
                values: new object[,]
                {
                    { 11, 2 },
                    { 34, 1 },
                    { 36, 1 },
                    { 38, 1 },
                    { 40, 2 },
                    { 41, 3 },
                    { 42, 4 },
                    { 43, 1 },
                    { 44, 2 },
                    { 45, 3 },
                    { 46, 4 },
                    { 47, 1 },
                    { 49, 1 },
                    { 51, 1 },
                    { 52, 2 },
                    { 53, 3 },
                    { 54, 4 },
                    { 55, 2 },
                    { 56, 3 },
                    { 57, 1 },
                    { 59, 1 },
                    { 61, 1 },
                    { 65, 1 },
                    { 69, 2 },
                    { 70, 1 },
                    { 71, 2 },
                    { 72, 1 },
                    { 73, 4 },
                    { 74, 5 },
                    { 75, 3 },
                    { 76, 3 }
                });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { false, 5, "To jest chemia 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 35,
                column: "Title",
                value: "To jest chemia 2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Level", "Title" },
                values: new object[] { true, "To jest chemia 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 37,
                column: "Title",
                value: "To jest chemia 2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { false, 6, "Edukacja dla bezpieczeństwa [wsip]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { true, 7, "Fizyka 1 [wsip]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 40,
                column: "Title",
                value: "Fizyka 2 [wsip]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 41,
                column: "Title",
                value: "Fizyka 3 [wsip]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 42,
                column: "Title",
                value: "Fizyka 4 [wsip]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Level", "Title" },
                values: new object[] { false, "Fizyka 1 [wsip]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 44,
                column: "Title",
                value: "Fizyka 2 [wsip]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 45,
                column: "Title",
                value: "Fizyka 3 [wsip]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 46,
                column: "Title",
                value: "Fizyka 4 [wsip]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 8, "Oblicza geografii 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 48,
                column: "Title",
                value: "Oblicza geografii 2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 49,
                column: "Title",
                value: "Oblicz geografii karty pracy 1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 50,
                column: "Title",
                value: "Oblicz geografii karty pracy 2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 9, "Historia [wsip] 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 52,
                column: "Title",
                value: "Historia [wsip] 2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 53,
                column: "Title",
                value: "Historia [wsip] 3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 54,
                column: "Title",
                value: "Historia [wsip] 4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 10, "Historia i teraźniejszość [wsip] 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 56,
                column: "Title",
                value: "Historia i teraźniejszość [wsip] 2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 11, "Informatyka [operon]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 58,
                column: "Title",
                value: "Informatyka dla szkół ponadgimnazjalnych [Migra]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 59,
                columns: new[] { "Level", "Title" },
                values: new object[] { true, "Informatyka [operon]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 60,
                column: "Title",
                value: "Informatyka dla szkół ponadgimnazjalnych [Migra]");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 61,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { false, 12, "NOWA MATeMAtyka 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 62,
                column: "Title",
                value: "NOWA MATeMAtyka 2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 63,
                column: "Title",
                value: "NOWA MATeMAtyka 3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 64,
                column: "Title",
                value: "NOWA MATeMAtyka 4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 65,
                columns: new[] { "Level", "Title" },
                values: new object[] { true, "NOWA MATeMAtyka 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 66,
                column: "Title",
                value: "NOWA MATeMAtyka 2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 67,
                column: "Title",
                value: "NOWA MATeMAtyka 3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 68,
                column: "Title",
                value: "NOWA MATeMAtyka 4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 69,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { false, 13, "Krok w przedsiębiorczość" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 70,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 14, "Krok w biznes i zarządzanie 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 71,
                column: "Title",
                value: "Krok w biznes i zarządzanie 2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 72,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 15, "Spotkania ze sztuką 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 73,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 16, "W centrum uwagi 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 74,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 16, "W centrum uwagi 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 75,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { true, 17, "Electronics" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 76,
                columns: new[] { "Level", "SubjectId", "Title" },
                values: new object[] { true, 17, "Electrician" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 77,
                column: "Title",
                value: "Software engineering");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 78,
                columns: new[] { "Level", "Title" },
                values: new object[] { false, "IT [english for IT]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 79,
                columns: new[] { "SubjectId", "Title" },
                values: new object[] { 11, "Informatyka w praktyce" });

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 18);
        }
    }
}
