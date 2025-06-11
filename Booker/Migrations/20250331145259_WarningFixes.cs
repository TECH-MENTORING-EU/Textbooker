using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class WarningFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Items",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<bool>(
                name: "Level",
                table: "Books",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "94085ea4-b1a7-4821-b83f-a0461f2a6ea7");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "09245240-08b6-48a3-a50d-ae5b4192ae3e");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "5c20fa47-97aa-458e-8c94-6ccab11ef510");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                column: "ConcurrencyStamp",
                value: "8ceae1fa-aa96-485e-844a-f87a5c38d5e1");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 5,
                column: "ConcurrencyStamp",
                value: "919a5b57-05dc-4ca7-989a-7348e6b7419b");

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 39, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9793), 54.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 61, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9854), 51.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 56, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9858), 47.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 1, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9861), 72.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 37, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9865), 58m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 7, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9869), 83.42857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 72, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9873), 38.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 6, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9877), 31.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 36, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9880), 54.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 18, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9884), 27.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 59, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9888), 83.14285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 11, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9891), 77.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 26, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9894), 38.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 44, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9898), 82.71428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 29, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9900), 78m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 47, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9904), 58m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 13, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9908), 27.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 60, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9911), 60.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 68, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9915), 22.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 64, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9918), 76.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 8, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9921), 71m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 23, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9924), 31.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 60, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9928), 76.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 47, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9931), 68m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 62, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9934), 64.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 26, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9938), 55.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 58, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9941), 82m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 69, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9944), 59.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 24, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9947), 41.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 33, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9950), 26.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 15, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9955), 30.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 5, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9957), 51m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 71, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9960), 85m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 15, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9964), 76.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 20, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9967), 74.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 50, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9971), 60.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 62, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9974), 54.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 18, new DateTime(2025, 3, 31, 16, 52, 59, 80, DateTimeKind.Local).AddTicks(9997), 60.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 50, new DateTime(2025, 3, 31, 16, 52, 59, 81, DateTimeKind.Local), 78.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 60, new DateTime(2025, 3, 31, 16, 52, 59, 81, DateTimeKind.Local).AddTicks(4), 76.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 11, new DateTime(2025, 3, 31, 16, 52, 59, 81, DateTimeKind.Local).AddTicks(8), 33.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 21, new DateTime(2025, 3, 31, 16, 52, 59, 81, DateTimeKind.Local).AddTicks(11), 42.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 67, new DateTime(2025, 3, 31, 16, 52, 59, 81, DateTimeKind.Local).AddTicks(15), 85.28571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 71, new DateTime(2025, 3, 31, 16, 52, 59, 81, DateTimeKind.Local).AddTicks(18), 82.57142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 71, new DateTime(2025, 3, 31, 16, 52, 59, 81, DateTimeKind.Local).AddTicks(21), 66m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 2, new DateTime(2025, 3, 31, 16, 52, 59, 81, DateTimeKind.Local).AddTicks(24), 56.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 42, new DateTime(2025, 3, 31, 16, 52, 59, 81, DateTimeKind.Local).AddTicks(27), 77.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 78, new DateTime(2025, 3, 31, 16, 52, 59, 81, DateTimeKind.Local).AddTicks(30), 63.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 28, new DateTime(2025, 3, 31, 16, 52, 59, 81, DateTimeKind.Local).AddTicks(34), 27.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 67, new DateTime(2025, 3, 31, 16, 52, 59, 81, DateTimeKind.Local).AddTicks(37), 83.14285714285714285714285714m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Items",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<bool>(
                name: "Level",
                table: "Books",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "b60c32af-3b1d-4afb-a38c-492a843446c5");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "dd4497ab-bbfd-419f-b141-00028859ecfe");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "4af4eabd-7d9c-4968-bc48-c5f5a97003be");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                column: "ConcurrencyStamp",
                value: "47fde348-0bb9-4535-9046-9dceba8c72d1");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 5,
                column: "ConcurrencyStamp",
                value: "7973b629-c99d-4ab7-b10d-9c7c7f351c52");

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 15, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(44), 40.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 72, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(102), 72m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 2, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(106), 56.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 73, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(109), 79.28571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 14, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(114), 46.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 3, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(117), 34.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 60, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(120), 52.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 62, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(125), 65.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 61, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(128), 31.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 51, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(131), 85.57142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 9, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(134), 80.14285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 8, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(159), 44.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 47, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(163), 39.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 39, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(166), 84m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 64, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(169), 54.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 11, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(173), 66.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 75, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(175), 55m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 71, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(179), 41m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 15, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(182), 26.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 67, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(186), 83.28571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 18, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(189), 66.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 24, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(192), 68.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 51, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(195), 61.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 67, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(199), 65m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 8, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(202), 60.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 68, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(205), 72.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 51, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(208), 53.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 8, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(211), 68.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 2, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(214), 54.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 58, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(217), 43m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 19, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(220), 77.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 11, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(225), 64.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 18, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(228), 47.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 18, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(231), 27m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 56, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(234), 46.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 66, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(238), 48.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 14, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(241), 22.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 43, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(244), 79.57142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 37, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(247), 56.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 58, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(250), 80.14285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 18, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(254), 61.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 67, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(257), 79.42857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 12, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(261), 48.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 32, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(264), 45.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 66, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(267), 75.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 36, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(271), 27.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 2, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(275), 25.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 34, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(278), 35.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 43, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(281), 32.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 4, new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(284), 52.714285714285714285714285714m });
        }
    }
}
