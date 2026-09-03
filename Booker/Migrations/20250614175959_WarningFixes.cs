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
                value: "ed35d162-1fcd-4bd0-9c5d-91eea43d742a");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "3ec5d9f6-ca4e-4011-9e9a-d9720ed87ded");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "9a5ae8da-9d64-443a-ade0-fe6bb552c61b");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                column: "ConcurrencyStamp",
                value: "90c50ca1-8852-4413-a0dd-a9f1b4c85ac7");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 5,
                column: "ConcurrencyStamp",
                value: "c76fee65-29c9-4db1-b5bd-dbac57d21e6a");

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 23, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6182), 59.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DateTime", "Price" },
                values: new object[] { new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6252), 42.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 39, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6259), 42.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DateTime", "Price" },
                values: new object[] { new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6263), 63m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 72, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6267), 38.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 5, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6274), 21.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 39, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6278), 50.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 34, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6284), 84.28571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 78, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6288), 83.85714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 68, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6294), 40.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 49, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6299), 56.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 40, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6303), 44.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 48, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6308), 28.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 34, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6312), 41.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 48, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6316), 67.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 7, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6321), 43.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 73, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6326), 64.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 66, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6355), 72.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 50, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6360), 74.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 50, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6365), 82.28571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 29, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6370), 32.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 61, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6374), 35.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 19, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6378), 23.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 1, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6383), 68.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 48, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6388), 75.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 37, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6392), 34.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 56, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6397), 53.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 29, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6401), 65.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 75, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6405), 29.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 40, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6410), 45.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 8, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6415), 72.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 61, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6420), 50.714285714285714285714285714m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 26, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6424), 76m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 63, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6430), 74.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 8, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6435), 33.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 37, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6438), 34m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 17, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6441), 85m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 53, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6446), 74.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 20, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6451), 45.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 41, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6455), 84.85714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 66, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6460), 43.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 9, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6465), 39.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 71, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6468), 51m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 75, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6473), 40.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 46, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6477), 36.428571428571428571428571429m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 31, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6482), 56.857142857142857142857142857m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 10, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6486), 57.142857142857142857142857143m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 58, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6491), 74.571428571428571428571428571m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 39, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6495), 24.285714285714285714285714286m });

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 10, new DateTime(2025, 6, 14, 19, 59, 58, 431, DateTimeKind.Local).AddTicks(6499), 79m });
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
                columns: new[] { "DateTime", "Price" },
                values: new object[] { new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(102), 72m });

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
                columns: new[] { "DateTime", "Price" },
                values: new object[] { new DateTime(2025, 2, 24, 14, 52, 33, 490, DateTimeKind.Local).AddTicks(109), 79.28571428571428571428571429m });

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
