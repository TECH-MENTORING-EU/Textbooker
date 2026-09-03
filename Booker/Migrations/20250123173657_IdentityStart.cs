using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class IdentityStart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Users_UserID",
                table: "Items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Items",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Items_UserID",
                table: "Items",
                newName: "IX_Items_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccessFailedCount", "ConcurrencyStamp", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { 0, "865958b1-4949-45b4-876a-1d5b689c3adb", false, false, null, null, null, null, null, false, null, false, "user1" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AccessFailedCount", "ConcurrencyStamp", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { 0, "961a03c8-6368-4537-b9f0-4905b44f1fbb", false, false, null, null, null, null, null, false, null, false, "user2" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AccessFailedCount", "ConcurrencyStamp", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { 0, "145d3563-9ee9-4528-8afb-719e0725c863", false, false, null, null, null, null, null, false, null, false, "user3" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AccessFailedCount", "ConcurrencyStamp", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { 0, "b78d1dac-7011-4dc8-adc1-f7395a4159cd", false, false, null, null, null, null, null, false, null, false, "user4" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AccessFailedCount", "ConcurrencyStamp", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { 0, "4af65730-29f6-4ea4-bf9f-4da9dbdd8c4c", false, false, null, null, null, null, null, false, null, false, "user5" });

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
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 74, new DateTime(2025, 1, 23, 18, 36, 57, 3, DateTimeKind.Local).AddTicks(7688), 42m });

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

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_AspNetUsers_UserId",
                table: "Items",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_AspNetUsers_UserId",
                table: "Items");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PhoneNumberConfirmed",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "Users");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Items",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Items_UserId",
                table: "Items",
                newName: "IX_Items_UserID");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

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
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 56, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9480), 83.14285714285714285714285714m });

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
                columns: new[] { "BookId", "DateTime", "Price" },
                values: new object[] { 19, new DateTime(2025, 1, 11, 19, 43, 18, 355, DateTimeKind.Local).AddTicks(9558), 20.428571428571428571428571429m });

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

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Nickname", "Password" },
                values: new object[] { "user1", "zaq1@WSX" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Nickname", "Password" },
                values: new object[] { "user2", "zaq1@WSX" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Nickname", "Password" },
                values: new object[] { "user3", "zaq1@WSX" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Nickname", "Password" },
                values: new object[] { "user4", "zaq1@WSX" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Nickname", "Password" },
                values: new object[] { "user5", "zaq1@WSX" });

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Users_UserID",
                table: "Items",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
