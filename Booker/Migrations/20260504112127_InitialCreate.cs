using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "ChatThreads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserAId = table.Column<int>(type: "int", nullable: false),
                    UserBId = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMessageUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatThreads", x => x.Id);
                });

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
                name: "Levels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Levels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmailDomain = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
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
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreFavoritesPublic = table.Column<bool>(type: "bit", nullable: false),
                    DisplayEmail = table.Column<bool>(type: "bit", nullable: false),
                    DisplayWhatsapp = table.Column<bool>(type: "bit", nullable: false),
                    FbMessenger = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    LevelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_Levels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Books_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DealId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewerId = table.Column<int>(type: "int", nullable: false),
                    RevieweeId = table.Column<int>(type: "int", nullable: false),
                    RatingValue = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRatings_AspNetUsers_RevieweeId",
                        column: x => x.RevieweeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRatings_AspNetUsers_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    CanChangeVisibility = table.Column<bool>(type: "bit", nullable: false),
                    Reserved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Items_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFavorites",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavorites", x => new { x.UserId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_UserFavorites_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserFavorites_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.InsertData(
                table: "Levels",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { -1, "Brak" },
                    { 1, "Podstawa" },
                    { 2, "Rozszerzenie" },
                    { 3, "Podstawa+Rozszerzenie" },
                    { 4, "Dwujęzyczny" }
                });

            migrationBuilder.InsertData(
                table: "Schools",
                columns: new[] { "Id", "CreatedAt", "DeactivatedAt", "EmailDomain", "IsActive", "Name" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "hogwart.edu.pl", true, "Hogwort" });

            migrationBuilder.InsertData(
                table: "Subjects",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { -1, "Brak" },
                    { 1, "Język polski" },
                    { 2, "Język angielski" },
                    { 3, "Język niemiecki" },
                    { 4, "Biologia" },
                    { 5, "Chemia" },
                    { 6, "EDB" },
                    { 7, "Fizyka" },
                    { 8, "Geografia" },
                    { 9, "Historia" },
                    { 10, "Historia i teraźniejszość" },
                    { 11, "Informatyka" },
                    { 12, "Matematyka" },
                    { 13, "Podstawy przedsiębiorczości" },
                    { 14, "Biznes i zarządzanie" },
                    { 15, "Plastyka" },
                    { 16, "WOS" },
                    { 17, "Język angielski zawodowy" },
                    { 18, "Edukacja obywatelska" }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "LevelId", "SubjectId", "Title" },
                values: new object[,]
                {
                    { -1, -1, -1, "Inna" },
                    { 1, 3, 1, "Ponad słowami 1 cz. 1" },
                    { 2, 3, 1, "Ponad słowami 1 cz. 2" },
                    { 3, 3, 1, "Ponad słowami 2 cz. 1" },
                    { 4, 3, 1, "Ponad słowami 2 cz. 2" },
                    { 5, 3, 1, "Ponad słowami 3 cz. 1" },
                    { 6, 3, 1, "Ponad słowami 3 cz. 2" },
                    { 7, 3, 1, "Ponad słowami 4" },
                    { 8, 3, 2, "Focus 2 Podręcznik" },
                    { 9, 3, 2, "Focus 3 Podręcznik" },
                    { 10, 3, 2, "Focus 4 Podręcznik" },
                    { 11, 3, 2, "Focus 5 Podręcznik" },
                    { 12, 3, 2, "Focus 2 Ćwiczenia" },
                    { 13, 3, 2, "Focus 3 Ćwiczenia" },
                    { 14, 3, 2, "Focus 4 Ćwiczenia" },
                    { 15, 3, 2, "Focus 5 Ćwiczenia" },
                    { 16, 3, 2, "My matura perspectives [nowa era]" },
                    { 17, 3, 2, "Repetytorium [Macmillan]" },
                    { 18, 2, 2, "Repetytorium maturzysty [Oxford]" },
                    { 19, 3, 2, "Repetytorium maturzysty [Cambridge, PWN]" },
                    { 20, 3, 3, "Welttour Deutsch 1" },
                    { 21, 3, 3, "Welttour Deutsch 2" },
                    { 22, 3, 3, "Welttour Deutsch 3" },
                    { 23, 3, 3, "Welttour Deutsch 4" },
                    { 24, 3, 3, "Effekt 1" },
                    { 25, 3, 3, "Effekt 2" },
                    { 26, 3, 3, "Effekt 3" },
                    { 27, 3, 3, "Effekt 4" },
                    { 28, 1, 4, "Biologia na czasie 1" },
                    { 29, 1, 4, "Biologia na czasie 2" },
                    { 30, 1, 4, "Biologia na czasie 3" },
                    { 31, 2, 4, "Biologia na czasie 1" },
                    { 32, 2, 4, "Biologia na czasie 2" },
                    { 33, 2, 4, "Biologia na czasie 3" },
                    { 34, 2, 4, "Biologia na czasie 4" },
                    { 35, 1, 5, "To jest chemia 1" },
                    { 36, 1, 5, "To jest chemia 2" },
                    { 37, 2, 5, "To jest chemia 1" },
                    { 38, 2, 5, "To jest chemia 2" },
                    { 39, 1, 6, "Edukacja dla bezpieczeństwa [wsip]" },
                    { 40, 2, 7, "Fizyka 1 [wsip]" },
                    { 41, 2, 7, "Fizyka 2 [wsip]" },
                    { 42, 2, 7, "Fizyka 3 [wsip]" },
                    { 43, 2, 7, "Fizyka 4 [wsip]" },
                    { 44, 1, 7, "Fizyka 1 [wsip]" },
                    { 45, 1, 7, "Fizyka 2 [wsip]" },
                    { 46, 1, 7, "Fizyka 3 [wsip]" },
                    { 47, 1, 7, "Fizyka 4 [wsip]" },
                    { 48, 1, 8, "Oblicza geografii 1" },
                    { 49, 1, 8, "Oblicza geografii 2" },
                    { 50, 1, 8, "Oblicza geografii karty pracy 1" },
                    { 51, 1, 8, "Oblicza geografii karty pracy 2" },
                    { 52, 1, 9, "Historia [wsip] 1" },
                    { 53, 1, 9, "Historia [wsip] 2" },
                    { 54, 1, 9, "Historia [wsip] 3" },
                    { 55, 1, 9, "Historia [wsip] 4" },
                    { 56, 1, 10, "Historia i teraźniejszość [wsip] 1" },
                    { 57, 1, 10, "Historia i teraźniejszość [wsip] 2" },
                    { 58, 1, 11, "Informatyka [operon]" },
                    { 59, 1, 11, "Informatyka dla szkół ponadgimnazjalnych [Migra]" },
                    { 60, 2, 11, "Informatyka [operon]" },
                    { 61, 2, 11, "Informatyka dla szkół ponadgimnazjalnych [Migra]" },
                    { 62, 1, 12, "NOWA MATeMAtyka 1" },
                    { 63, 1, 12, "NOWA MATeMAtyka 2" },
                    { 64, 1, 12, "NOWA MATeMAtyka 3" },
                    { 65, 1, 12, "NOWA MATeMAtyka 4" },
                    { 66, 3, 12, "NOWA MATeMAtyka 1" },
                    { 67, 3, 12, "NOWA MATeMAtyka 2" },
                    { 68, 3, 12, "NOWA MATeMAtyka 3" },
                    { 69, 3, 12, "NOWA MATeMAtyka 4" },
                    { 70, 1, 13, "Krok w przedsiębiorczość" },
                    { 71, 1, 14, "Krok w biznes i zarządzanie 1" },
                    { 72, 1, 14, "Krok w biznes i zarządzanie 2" },
                    { 73, 1, 15, "Spotkania ze sztuką 1" },
                    { 74, 1, 18, "Masz wpływ 1" },
                    { 75, 1, 16, "W centrum uwagi 1" },
                    { 76, 1, 16, "W centrum uwagi 2" },
                    { 77, -1, 17, "Electronics" },
                    { 78, -1, 17, "Electrician" },
                    { 79, -1, 17, "Software engineering" },
                    { 80, -1, 17, "Computing" },
                    { 81, -1, 17, "Mechanical engineering" },
                    { 82, -1, 17, "Mechanics" },
                    { 83, -1, 17, "Environmental Science" },
                    { 84, -1, 17, "IT [english for IT]" },
                    { 85, 2, 11, "Informatyka w praktyce" }
                });

            migrationBuilder.InsertData(
                table: "BookGrades",
                columns: new[] { "BookId", "GradeId" },
                values: new object[,]
                {
                    { -1, 1 },
                    { -1, 2 },
                    { -1, 3 },
                    { -1, 4 },
                    { -1, 5 },
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 2 },
                    { 4, 2 },
                    { 4, 3 },
                    { 5, 3 },
                    { 6, 3 },
                    { 6, 4 },
                    { 7, 4 },
                    { 7, 5 },
                    { 8, 1 },
                    { 8, 2 },
                    { 8, 3 },
                    { 9, 1 },
                    { 9, 2 },
                    { 9, 3 },
                    { 10, 1 },
                    { 10, 2 },
                    { 10, 3 },
                    { 10, 4 },
                    { 11, 3 },
                    { 11, 4 },
                    { 11, 5 },
                    { 12, 1 },
                    { 12, 2 },
                    { 12, 3 },
                    { 13, 1 },
                    { 13, 2 },
                    { 13, 3 },
                    { 14, 1 },
                    { 14, 2 },
                    { 14, 3 },
                    { 14, 4 },
                    { 15, 3 },
                    { 15, 4 },
                    { 15, 5 },
                    { 16, 4 },
                    { 16, 5 },
                    { 17, 5 },
                    { 18, 5 },
                    { 19, 5 },
                    { 20, 1 },
                    { 21, 1 },
                    { 21, 2 },
                    { 22, 3 },
                    { 23, 4 },
                    { 23, 5 },
                    { 24, 1 },
                    { 24, 2 },
                    { 25, 2 },
                    { 25, 3 },
                    { 26, 3 },
                    { 26, 4 },
                    { 27, 4 },
                    { 27, 5 },
                    { 28, 1 },
                    { 29, 2 },
                    { 29, 3 },
                    { 30, 3 },
                    { 30, 4 },
                    { 31, 1 },
                    { 32, 2 },
                    { 33, 3 },
                    { 34, 4 },
                    { 35, 1 },
                    { 35, 2 },
                    { 35, 3 },
                    { 36, 2 },
                    { 36, 3 },
                    { 36, 4 },
                    { 37, 1 },
                    { 37, 2 },
                    { 37, 3 },
                    { 38, 2 },
                    { 38, 3 },
                    { 38, 4 },
                    { 38, 5 },
                    { 39, 1 },
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
                    { 48, 2 },
                    { 49, 2 },
                    { 49, 3 },
                    { 49, 4 },
                    { 50, 1 },
                    { 50, 2 },
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
                    { 58, 2 },
                    { 59, 2 },
                    { 59, 3 },
                    { 59, 4 },
                    { 60, 1 },
                    { 60, 2 },
                    { 61, 2 },
                    { 61, 3 },
                    { 61, 4 },
                    { 62, 1 },
                    { 62, 2 },
                    { 63, 2 },
                    { 63, 3 },
                    { 64, 3 },
                    { 64, 4 },
                    { 65, 4 },
                    { 65, 5 },
                    { 66, 1 },
                    { 66, 2 },
                    { 67, 2 },
                    { 67, 3 },
                    { 68, 3 },
                    { 68, 4 },
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
                    { 77, 3 },
                    { 77, 4 },
                    { 78, 3 },
                    { 78, 4 },
                    { 79, 3 },
                    { 79, 4 },
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

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SchoolId",
                table: "AspNetUsers",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BookGrades_GradeId",
                table: "BookGrades",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_LevelId",
                table: "Books",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_SubjectId",
                table: "Books",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_DealId_CreatedUtc",
                table: "ChatMessages",
                columns: new[] { "DealId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_UserId",
                table: "ChatMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreads_ChannelId",
                table: "ChatThreads",
                column: "ChannelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreads_UserAId_UserBId",
                table: "ChatThreads",
                columns: new[] { "UserAId", "UserBId" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_BookId",
                table: "Items",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_UserId",
                table: "Items",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavorites_ItemId",
                table: "UserFavorites",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_RevieweeId",
                table: "UserRatings",
                column: "RevieweeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_ReviewerId_RevieweeId",
                table: "UserRatings",
                columns: new[] { "ReviewerId", "RevieweeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "BookGrades");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ChatThreads");

            migrationBuilder.DropTable(
                name: "UserFavorites");

            migrationBuilder.DropTable(
                name: "UserRatings");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropTable(
                name: "Levels");

            migrationBuilder.DropTable(
                name: "Subjects");
        }
    }
}
