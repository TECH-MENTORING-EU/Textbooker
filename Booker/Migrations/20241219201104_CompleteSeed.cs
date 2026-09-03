using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class CompleteSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "1", "Fizyka", "Fizyka 1 [wsip]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "2", "Fizyka", "Fizyka 2 [wsip]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "3", "Fizyka", "Fizyka 3 [wsip]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "4/5", "Fizyka", "Fizyka 4 [wsip]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "1/2", "Geografia", "Oblicza geografii 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "3/4", "Geografia", "Oblicza geografii 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "1/2", "Geografia", "Oblicz geografii karty pracy 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "3/4", "Geografia", "Oblicz geografii karty pracy 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "1", "Historia", "Historia [wsip] 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 52,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "2", "Historia", "Historia [wsip] 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "3", "Historia", "Historia [wsip] 3" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "4", "Historia", "Historia [wsip] 4" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "2", true, "HiT", "Historia i teraźniejszość [wsip] 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "3", true, "HiT", "Historia i teraźniejszość [wsip] 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "Subject", "Title" },
                values: new object[] { "Informatyka", "Informatyka [operon]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "3/4", "Informatyka", "Informatyka dla szkół ponadgimnazjalnych [Migra]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 59,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "1/2", false, "Informatyka", "Informatyka [operon]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 60,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "3/4", false, "Informatyka", "Informatyka dla szkół ponadgimnazjalnych [Migra]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 61,
                column: "Level",
                value: true);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 62,
                column: "Level",
                value: true);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 63,
                column: "Level",
                value: true);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 64,
                column: "Level",
                value: true);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 65,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "1/2", false, "Matematyka", "NOWA MATeMAtyka 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 66,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "2/3", false, "Matematyka", "NOWA MATeMAtyka 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 67,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "3/4", false, "Matematyka", "NOWA MATeMAtyka 3" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 68,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "4/5", false, "Matematyka", "NOWA MATeMAtyka 4" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 69,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "2/3", "Podstawy przedsiębiorczości", "Krok w przedsiębiorczość" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 70,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "1", "Biznes i zarządzanie", "Krok w biznes i zarządzanie 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 71,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "2", true, "Biznes i zarządzanie", "Krok w biznes i zarządzanie 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 72,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "1", true, "Plastyka", "Spotkania ze sztuką 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 73,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "4", true, "WOS", "W centrum uwagi 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 74,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "5", true, "WOS", "W centrum uwagi 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 75,
                column: "Title",
                value: "Electronics");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 76,
                column: "Title",
                value: "Electrician");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 77,
                column: "Title",
                value: "Software engineering");

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Grade", "Level", "Subject", "Title" },
                values: new object[,]
                {
                    { 78, "3/4", false, "Angielski zawodowy", "Computing" },
                    { 79, "3/4", false, "Angielski zawodowy", "Mechanical engineering" },
                    { 80, "3/4", false, "Angielski zawodowy", "Mechanics" },
                    { 81, "3/4", false, "Angielski zawodowy", "Enviromental Science" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Nickname", "Password", "Photo", "School" },
                values: new object[,]
                {
                    { 1, "user1@gmail.com", "user1", "zaq1@WSX", "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_1280.png", "Śl.TZN" },
                    { 2, "user2@gmail.com", "user2", "zaq1@WSX", "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_1280.png", "Śl.TZN" },
                    { 3, "user3@gmail.com", "user3", "zaq1@WSX", "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_1280.png", "Śl.TZN" },
                    { 4, "user4@gmail.com", "user4", "zaq1@WSX", "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_1280.png", "Śl.TZN" },
                    { 5, "user5@gmail.com", "user5", "zaq1@WSX", "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_1280.png", "Śl.TZN" }
                });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "BookId", "DateTime", "Description", "Photo", "Price", "State", "UserID" },
                values: new object[,]
                {
                    { 1, 68, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9506), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 23.142857142857142857142857143m, "bardzo dobry", 1 },
                    { 2, 59, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9563), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 36.285714285714285714285714286m, "bardzo dobry", 2 },
                    { 3, 64, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9569), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 62.714285714285714285714285714m, "bardzo dobry", 3 },
                    { 4, 48, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9572), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 48.857142857142857142857142857m, "bardzo dobry", 4 },
                    { 5, 12, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9575), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 32.857142857142857142857142857m, "bardzo dobry", 5 },
                    { 6, 60, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9579), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 79m, "bardzo dobry", 1 },
                    { 7, 50, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9582), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 24.714285714285714285714285714m, "bardzo dobry", 2 },
                    { 8, 22, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9586), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 77.714285714285714285714285714m, "bardzo dobry", 3 },
                    { 9, 14, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9590), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 82.85714285714285714285714286m, "bardzo dobry", 4 },
                    { 10, 17, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9593), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 36.571428571428571428571428571m, "bardzo dobry", 5 },
                    { 11, 51, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9596), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 76.857142857142857142857142857m, "bardzo dobry", 1 },
                    { 12, 57, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9600), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 62.285714285714285714285714286m, "bardzo dobry", 2 },
                    { 13, 19, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9603), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 20.285714285714285714285714286m, "bardzo dobry", 3 },
                    { 14, 23, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9606), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 53.714285714285714285714285714m, "bardzo dobry", 4 },
                    { 15, 60, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9609), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 54.142857142857142857142857143m, "bardzo dobry", 5 },
                    { 16, 36, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9613), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 49.285714285714285714285714286m, "bardzo dobry", 1 },
                    { 17, 69, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9616), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 49.571428571428571428571428571m, "bardzo dobry", 2 },
                    { 18, 70, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9619), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 55m, "bardzo dobry", 3 },
                    { 19, 27, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9622), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 58.857142857142857142857142857m, "bardzo dobry", 4 },
                    { 20, 17, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9625), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 72.428571428571428571428571429m, "bardzo dobry", 5 },
                    { 21, 79, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9628), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 83.28571428571428571428571429m, "bardzo dobry", 1 },
                    { 22, 44, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9631), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 50.571428571428571428571428571m, "bardzo dobry", 2 },
                    { 23, 23, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9635), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 53m, "bardzo dobry", 3 },
                    { 24, 30, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9638), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 78.428571428571428571428571429m, "bardzo dobry", 4 },
                    { 25, 42, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9641), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 83.14285714285714285714285714m, "bardzo dobry", 5 },
                    { 26, 11, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9644), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 35.857142857142857142857142857m, "bardzo dobry", 1 },
                    { 27, 29, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9648), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 42.142857142857142857142857143m, "bardzo dobry", 2 },
                    { 28, 51, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9651), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 70.285714285714285714285714286m, "bardzo dobry", 3 },
                    { 29, 19, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9654), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 73.142857142857142857142857143m, "bardzo dobry", 4 },
                    { 30, 49, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9657), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 52m, "bardzo dobry", 5 },
                    { 31, 56, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9660), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 32m, "bardzo dobry", 1 },
                    { 32, 79, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9663), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 33.857142857142857142857142857m, "bardzo dobry", 2 },
                    { 33, 19, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9667), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 80m, "bardzo dobry", 3 },
                    { 34, 24, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9670), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 34m, "bardzo dobry", 4 },
                    { 35, 29, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9672), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 42m, "bardzo dobry", 5 },
                    { 36, 6, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9676), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 30.857142857142857142857142857m, "bardzo dobry", 1 },
                    { 37, 62, new DateTime(2024, 12, 19, 21, 11, 4, 454, DateTimeKind.Local).AddTicks(9679), "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.", "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 29.285714285714285714285714286m, "bardzo dobry", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "1/2", "Geografia", "Oblicza geografii 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "3/4", "Geografia", "Oblicza geografii 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "1/2", "Geografia", "Oblicz geografii karty pracy 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "3/4", "Geografia", "Oblicz geografii karty pracy 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "1", "Historia", "Historia [wsip] 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "2", "Historia", "Historia [wsip] 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "3", "Historia", "Historia [wsip] 3" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "4", "Historia", "Historia [wsip] 4" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "2", "HiT", "Historia i teraźniejszość [wsip] 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 52,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "3", "HiT", "Historia i teraźniejszość [wsip] 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "1/2", "Informatyka", "Informatyka [operon]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "3/4", "Informatyka", "Informatyka dla szkół ponadgimnazjalnych [Migra]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "1/2", false, "Informatyka", "Informatyka [operon]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "3/4", false, "Informatyka", "Informatyka dla szkół ponadgimnazjalnych [Migra]" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "Subject", "Title" },
                values: new object[] { "Matematyka", "NOWA MATeMAtyka 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "2/3", "Matematyka", "NOWA MATeMAtyka 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 59,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "3/4", true, "Matematyka", "NOWA MATeMAtyka 3" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 60,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "4/5", true, "Matematyka", "NOWA MATeMAtyka 4" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 61,
                column: "Level",
                value: false);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 62,
                column: "Level",
                value: false);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 63,
                column: "Level",
                value: false);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 64,
                column: "Level",
                value: false);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 65,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "2/3", true, "Podstawy przedsiębiorczości", "Krok w przedsiębiorczość" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 66,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "1", true, "Biznes i zarządzanie", "Krok w biznes i zarządzanie 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 67,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "2", true, "Biznes i zarządzanie", "Krok w biznes i zarządzanie 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 68,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "1", true, "Plastyka", "Spotkania ze sztuką 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 69,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "4", "WOS", "W centrum uwagi 1" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 70,
                columns: new[] { "Grade", "Subject", "Title" },
                values: new object[] { "5", "WOS", "W centrum uwagi 2" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 71,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "3/4", false, "Angielski zawodowy", "Electronics" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 72,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "3/4", false, "Angielski zawodowy", "Electrician" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 73,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "3/4", false, "Angielski zawodowy", "Software engineering" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 74,
                columns: new[] { "Grade", "Level", "Subject", "Title" },
                values: new object[] { "3/4", false, "Angielski zawodowy", "Computing" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 75,
                column: "Title",
                value: "Mechanical engineering");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 76,
                column: "Title",
                value: "Mechanics");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 77,
                column: "Title",
                value: "Enviromental Science");
        }
    }
}
