using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class GradeRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookGrades");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
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
                    GradeId = table.Column<int>(type: "int", nullable: false)
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
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "174cdadd-9313-4490-bfe8-ec5e8ed021a2");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "cdf967bd-0141-4684-a610-3c73585598df");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "adcdc119-b8b4-4b72-8a19-e72647499b02");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                column: "ConcurrencyStamp",
                value: "121fcf76-a889-4322-82d6-5293236cb093");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 5,
                column: "ConcurrencyStamp",
                value: "22d2ade9-2628-4abb-a13a-030368b4d716");

            migrationBuilder.InsertData(
                table: "Grades",
                columns: new[] { "Id", "GradeNumber" },
                values: new object[,]
                {
                    { 1, "1" },
                    { 2, "2" },
                    { 3, "3" },
                    { 4, "4" },
                    { 5, "5" }
                });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 3, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(1974), 73.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 74, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2035), 79.28571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 3, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2042), 64.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 32, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2048), 31.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 46, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2054), 36.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 11, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2060), 68.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 76, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2065), 76.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 29, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2071), 82.42857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 21, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2077), 25.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 77, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2083), 71.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 66, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2088), 56.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 12, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2095), 23.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 73, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2101), 30.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "DateTime", "Price" },
                values: new object[] { new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2107), 42.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 53, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2112), 83.28571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 46, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2118), 47.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 35, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2124), 79m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 69, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2131), 50.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 11, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2137), 30.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 69, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2142), 65m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 73, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2147), 28.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 61, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2152), 42.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 31, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2159), 24.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 1, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2165), 78.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 8, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2170), 74.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 50, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2175), 53.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 24, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2181), 45.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 9, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2186), 22.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 48, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2209), 51.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 42, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2217), 62.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 74, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2221), 57.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 20, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2227), 47.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 27, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2233), 72.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 4, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2238), 61.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 69, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2245), 62.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 34, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2251), 79.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 39, new DateTime(2025, 2, 18, 22, 11, 40, 8, DateTimeKind.Local).AddTicks(2255), 41m });

            migrationBuilder.InsertData(
                table: "BookGrades",
                columns: new[] { "BookId", "GradeId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 2 },
                    { 4, 2 },
                    { 5, 3 },
                    { 6, 4 },
                    { 7, 5 },
                    { 8, 1 },
                    { 9, 1 },
                    { 10, 2 },
                    { 11, 2 },
                    { 12, 3 },
                    { 13, 3 },
                    { 14, 4 },
                    { 15, 4 },
                    { 16, 5 },
                    { 17, 5 },
                    { 18, 5 },
                    { 19, 5 },
                    { 20, 1 },
                    { 21, 2 },
                    { 22, 3 },
                    { 23, 4 },
                    { 24, 1 },
                    { 25, 2 },
                    { 26, 3 },
                    { 27, 4 },
                    { 28, 1 },
                    { 29, 2 },
                    { 30, 3 },
                    { 31, 1 },
                    { 32, 2 },
                    { 33, 3 },
                    { 34, 1 },
                    { 35, 2 },
                    { 36, 3 },
                    { 37, 4 },
                    { 38, 1 },
                    { 39, 1 },
                    { 40, 2 },
                    { 41, 3 },
                    { 42, 4 },
                    { 43, 1 },
                    { 44, 2 },
                    { 45, 3 },
                    { 46, 4 },
                    { 47, 1 },
                    { 48, 2 },
                    { 49, 1 },
                    { 50, 2 },
                    { 51, 1 },
                    { 52, 2 },
                    { 53, 3 },
                    { 54, 4 },
                    { 55, 2 },
                    { 56, 3 },
                    { 57, 1 },
                    { 58, 2 },
                    { 59, 3 },
                    { 60, 4 },
                    { 61, 1 },
                    { 62, 2 },
                    { 63, 3 },
                    { 64, 4 },
                    { 65, 1 },
                    { 66, 2 },
                    { 67, 3 },
                    { 68, 4 },
                    { 69, 2 },
                    { 70, 1 },
                    { 71, 2 },
                    { 72, 1 },
                    { 73, 4 },
                    { 74, 5 },
                    { 75, 3 },
                    { 76, 3 },
                    { 77, 3 },
                    { 78, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookGrades");

            migrationBuilder.DropTable(
                name: "Grades");

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
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "865958b1-4949-45b4-876a-1d5b689c3adb");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "961a03c8-6368-4537-b9f0-4905b44f1fbb");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "145d3563-9ee9-4528-8afb-719e0725c863");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                column: "ConcurrencyStamp",
                value: "b78d1dac-7011-4dc8-adc1-f7395a4159cd");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 5,
                column: "ConcurrencyStamp",
                value: "4af65730-29f6-4ea4-bf9f-4da9dbdd8c4c");

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
                values: new object[] { 8, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7517), 79.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 29, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7581), 57.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 74, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7587), 74.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 35, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7631), 21.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 50, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7636), 22.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 53, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7642), 29.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 4, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7652), 34.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 47, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7658), 20m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 63, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7662), 63m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 41, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7668), 77.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 61, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7673), 38.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 4, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7678), 47.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 77, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7683), 38m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "DateTime", "Price" },
                values: new object[] { new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7688), 42m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 24, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7694), 23.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 15, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7700), 20.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 66, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7705), 69.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 60, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7711), 77.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 33, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7715), 21.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 74, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7721), 40.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 45, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7727), 82.85714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 73, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7733), 37.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 3, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7738), 40.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 32, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7743), 27.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 59, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7748), 69.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 10, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7753), 41.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 7, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7759), 29.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 6, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7764), 30.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 36, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7769), 79.42857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 15, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7774), 84.85714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 12, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7779), 39m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 72, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7784), 80.14285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 75, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7789), 63m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 25, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7794), 63.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 21, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7798), 44.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 77, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7803), 22.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 10, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7809), 33.714285714285714285714285714m });

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
        }
    }
}
