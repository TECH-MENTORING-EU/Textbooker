using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class MultiGradeStart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Books");

            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GradeNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookGrades",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookGrades", x => new { x.BookId, x.GradeId });
                    table.ForeignKey(
                        name: "FK_BookGrades_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookGrades_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 78,
                columns: new[] { "Level", "Title" },
                values: new object[] { true, "IT [english for IT]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 79,
                columns: new[] { "Subject", "Title" },
                values: new object[] { "Informatyka", "Informatyka w praktyce" });

            migrationBuilder.InsertData(
                table: "Grades",
                columns: new[] { "Id", "GradeNumber" },
                values: new object[,]
                {
                    { "1", "1" },
                    { "2", "2" },
                    { "3", "3" },
                    { "4", "4" },
                    { "5", "5" }
                });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 73, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9369), 43m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 3, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9412), 52.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 40, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9416), 56.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 21, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9419), 35m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 56, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9422), 67.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 61, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9425), 31m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 35, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9428), 38.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 72, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9432), 63.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 43, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9434), 62.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 56, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9437), 56.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 21, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9440), 20.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 10, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9442), 49.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 23, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9445), 45.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 9, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9448), 32.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 34, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9451), 74.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 2, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9455), 81.57142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 44, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9458), 46.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 44, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9461), 68.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 70, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9463), 61.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 56, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9466), 36.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 74, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9469), 75.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 26, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9471), 61.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 41, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9475), 73m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 26, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9477), 69.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "BookId", "DateTime" },
                values: new object[] { 56, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9480) });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 55, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9483), 53.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 51, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9510), 74.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 62, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9555), 25.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "DateTime", "Price" },
                values: new object[] { new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9558), 20.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 25, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9561), 22m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 71, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9564), 69.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 38, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9567), 23.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 40, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9570), 58.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 46, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9572), 32.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 1, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9575), 22.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 45, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9578), 38.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 16, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9580), 68.857142857142857142857142857m });

            migrationBuilder.InsertData(
                table: "BookGrades",
                columns: new[] { "BookId", "GradeId" },
                values: new object[,]
                {
                    { 1, "1" },
                    { 2, "1" },
                    { 3, "2" },
                    { 4, "2" },
                    { 5, "3" },
                    { 6, "4" },
                    { 7, "5" },
                    { 8, "1" },
                    { 9, "1" },
                    { 10, "2" },
                    { 11, "2" },
                    { 12, "3" },
                    { 13, "3" },
                    { 14, "4" },
                    { 15, "4" },
                    { 16, "5" },
                    { 17, "5" },
                    { 18, "5" },
                    { 19, "5" },
                    { 20, "1" },
                    { 21, "2" },
                    { 22, "3" },
                    { 23, "4" },
                    { 24, "1" },
                    { 25, "2" },
                    { 26, "3" },
                    { 27, "4" },
                    { 28, "1" },
                    { 29, "2" },
                    { 30, "3" },
                    { 31, "1" },
                    { 32, "2" },
                    { 33, "3" },
                    { 34, "1" },
                    { 35, "2" },
                    { 36, "3" },
                    { 37, "4" },
                    { 38, "1" },
                    { 39, "1" },
                    { 40, "2" },
                    { 41, "3" },
                    { 42, "4" },
                    { 43, "1" },
                    { 44, "2" },
                    { 45, "3" },
                    { 46, "4" },
                    { 47, "1" },
                    { 48, "2" },
                    { 49, "1" },
                    { 50, "2" },
                    { 51, "1" },
                    { 52, "2" },
                    { 53, "3" },
                    { 54, "4" },
                    { 55, "2" },
                    { 56, "3" },
                    { 57, "1" },
                    { 58, "2" },
                    { 59, "3" },
                    { 60, "4" },
                    { 61, "1" },
                    { 62, "2" },
                    { 63, "3" },
                    { 64, "4" },
                    { 65, "1" },
                    { 66, "2" },
                    { 67, "3" },
                    { 68, "4" },
                    { 69, "2" },
                    { 70, "1" },
                    { 71, "2" },
                    { 72, "1" },
                    { 73, "4" },
                    { 74, "5" },
                    { 75, "3" },
                    { 76, "3" },
                    { 77, "3" },
                    { 78, "3" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookGrades_GradeId",
                table: "BookGrades",
                column: "GradeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookGrades");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                column: "Grade",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                column: "Grade",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                column: "Grade",
                value: "2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4,
                column: "Grade",
                value: "2/3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5,
                column: "Grade",
                value: "3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6,
                column: "Grade",
                value: "4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7,
                column: "Grade",
                value: "5");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8,
                column: "Grade",
                value: "Zależnie od poziomu");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 9,
                column: "Grade",
                value: "Zależnie od poziomu");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 10,
                column: "Grade",
                value: "Zależnie od poziomu");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 11,
                column: "Grade",
                value: "Zależnie od poziomu");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 12,
                column: "Grade",
                value: "Zależnie od poziomu");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 13,
                column: "Grade",
                value: "Zależnie od poziomu");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 14,
                column: "Grade",
                value: "Zależnie od poziomu");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 15,
                column: "Grade",
                value: "Zależnie od poziomu");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 16,
                column: "Grade",
                value: "4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 17,
                column: "Grade",
                value: "5");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 18,
                column: "Grade",
                value: "5");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 19,
                column: "Grade",
                value: "5");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 20,
                column: "Grade",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 21,
                column: "Grade",
                value: "2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 22,
                column: "Grade",
                value: "3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 23,
                column: "Grade",
                value: "4/5");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 24,
                column: "Grade",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 25,
                column: "Grade",
                value: "2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 26,
                column: "Grade",
                value: "3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 27,
                column: "Grade",
                value: "4/5");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 28,
                column: "Grade",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 29,
                column: "Grade",
                value: "2/3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 30,
                column: "Grade",
                value: "4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 31,
                column: "Grade",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 32,
                column: "Grade",
                value: "2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 33,
                column: "Grade",
                value: "3/4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 34,
                column: "Grade",
                value: "1,2,3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 35,
                column: "Grade",
                value: "4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 36,
                column: "Grade",
                value: "1,2,3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 37,
                column: "Grade",
                value: "4,5");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 38,
                column: "Grade",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 39,
                column: "Grade",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 40,
                column: "Grade",
                value: "2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 41,
                column: "Grade",
                value: "3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 42,
                column: "Grade",
                value: "4/5");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 43,
                column: "Grade",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 44,
                column: "Grade",
                value: "2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 45,
                column: "Grade",
                value: "3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 46,
                column: "Grade",
                value: "4/5");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 47,
                column: "Grade",
                value: "1/2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 48,
                column: "Grade",
                value: "3/4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 49,
                column: "Grade",
                value: "1/2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 50,
                column: "Grade",
                value: "3/4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 51,
                column: "Grade",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 52,
                column: "Grade",
                value: "2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 53,
                column: "Grade",
                value: "3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 54,
                column: "Grade",
                value: "4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 55,
                column: "Grade",
                value: "2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 56,
                column: "Grade",
                value: "3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 57,
                column: "Grade",
                value: "1/2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 58,
                column: "Grade",
                value: "3/4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 59,
                column: "Grade",
                value: "1/2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 60,
                column: "Grade",
                value: "3/4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 61,
                column: "Grade",
                value: "1/2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 62,
                column: "Grade",
                value: "2/3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 63,
                column: "Grade",
                value: "3/4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 64,
                column: "Grade",
                value: "4/5");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 65,
                column: "Grade",
                value: "1/2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 66,
                column: "Grade",
                value: "2/3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 67,
                column: "Grade",
                value: "3/4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 68,
                column: "Grade",
                value: "4/5");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 69,
                column: "Grade",
                value: "2/3");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 70,
                column: "Grade",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 71,
                column: "Grade",
                value: "2");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 72,
                column: "Grade",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 73,
                column: "Grade",
                value: "4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 74,
                column: "Grade",
                value: "5");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 75,
                column: "Grade",
                value: "3/4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 76,
                column: "Grade",
                value: "3/4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 77,
                column: "Grade",
                value: "3/4");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 78,
                columns: new[] { "Grade", "Level", "Title" },
                values: new object[] { "3/4", false, "Computing" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 79,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "3/4", "Angielski zawodowy", "Mechanical engineering" });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Grade", "Level", "Subject", "Title" },
                values: new object[,]
                {
                    { 80, "3/4", false, "Angielski zawodowy", "Mechanics" },
                    { 81, "3/4", false, "Angielski zawodowy", "Enviromental Science" }
                });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 68, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9506), 23.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 59, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9563), 36.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 64, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9569), 62.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 48, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9572), 48.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 12, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9575), 32.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 60, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9579), 79m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 50, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9582), 24.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 22, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9586), 77.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 14, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9590), 82.85714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 17, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9593), 36.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 51, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9596), 76.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 57, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9600), 62.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 19, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9603), 20.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 23, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9606), 53.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 60, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9609), 54.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 36, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9613), 49.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 69, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9616), 49.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 70, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9619), 55m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 27, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9622), 58.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 17, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9625), 72.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 79, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9628), 83.28571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 44, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9631), 50.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 23, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9635), 53m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 30, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9638), 78.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "BookId", "DateTime" },
                values: new object[] { 42, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9641) });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 11, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9644), 35.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 29, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9648), 42.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 51, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9651), 70.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "DateTime", "Price" },
                values: new object[] { new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9654), 73.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 49, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9657), 52m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 56, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9660), 32m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 79, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9663), 33.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 19, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9667), 80m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 24, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9670), 34m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 29, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9672), 42m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 6, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9676), 30.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 62, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9679), 29.285714285714285714285714286m });
        }
    }
}
