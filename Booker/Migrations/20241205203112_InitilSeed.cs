using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Booker.Migrations
{
    /// <inheritdoc />
    public partial class InitilSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Grade", "Level", "Subject", "Title" },
                values: new object[,]
                {
                    { 1, "1", true, "Polski", "Ponad słowami 1 cz. 1" },
                    { 2, "1", true, "Polski", "Ponad słowami 1 cz. 2" },
                    { 3, "2", true, "Polski", "Ponad słowami 2 cz. 1" },
                    { 4, "2/3", true, "Polski", "Ponad słowami 2 cz. 2" },
                    { 5, "3", true, "Polski", "Ponad słowami 3 cz. 1" },
                    { 6, "4", true, "Polski", "Ponad słowami 3 cz. 2" },
                    { 7, "5", true, "Polski", "Ponad słowami 4" },
                    { 8, "Zależnie od poziomu", true, "Język angielski", "Focus 2 Podręcznik" },
                    { 9, "Zależnie od poziomu", true, "Język angielski", "Focus 3 Podręcznik" },
                    { 10, "Zależnie od poziomu", true, "Język angielski", "Focus 4 Podręcznik" },
                    { 11, "Zależnie od poziomu", true, "Język angielski", "Focus 5 Podręcznik" },
                    { 12, "Zależnie od poziomu", true, "Język angielski", "Focus 2 Ćwiczenia" },
                    { 13, "Zależnie od poziomu", true, "Język angielski", "Focus 3 Ćwiczenia" },
                    { 14, "Zależnie od poziomu", true, "Język angielski", "Focus 4 Ćwiczenia" },
                    { 15, "Zależnie od poziomu", true, "Język angielski", "Focus 5 Ćwiczenia" },
                    { 16, "4", true, "Język angielski", "My matura perspectives [nowa era]" },
                    { 17, "5", true, "Język angielski", "Repetytorium [Macmillan]" },
                    { 18, "5", false, "Język angielski", "Repetytorium maturzysty [Oxford]" },
                    { 19, "5", true, "Język angielski", "Repetytorium maturzysty [Cambridge, PWN]" },
                    { 20, "1", true, "Język Niemiecki", "Welttour Deutsch 1" },
                    { 21, "2", true, "Język Niemiecki", "Welttour Deutsch 2" },
                    { 22, "3", true, "Język Niemiecki", "Welttour Deutsch 3" },
                    { 23, "4/5", true, "Język Niemiecki", "Welttour Deutsch 4" },
                    { 24, "1", true, "Język Niemiecki", "Effekt 1" },
                    { 25, "2", true, "Język Niemiecki", "Effekt 2" },
                    { 26, "3", true, "Język Niemiecki", "Effekt 3" },
                    { 27, "4/5", true, "Język Niemiecki", "Effekt 4" },
                    { 28, "1", true, "Biologia", "Biologia na czasie 1" },
                    { 29, "2/3", true, "Biologia", "Biologia na czasie 2" },
                    { 30, "4", true, "Biologia", "Biologia na czasie 3" },
                    { 31, "1", false, "Biologia", "Biologia na czasie 1" },
                    { 32, "2", false, "Biologia", "Biologia na czasie 2" },
                    { 33, "3/4", false, "Biologia", "Biologia na czasie 3" },
                    { 34, "1,2,3", true, "Chemia", "To jest chemia 1" },
                    { 35, "4", true, "Chemia", "To jest chemia 2" },
                    { 36, "1,2,3", false, "Chemia", "To jest chemia 1" },
                    { 37, "4,5", false, "Chemia", "To jest chemia 2" },
                    { 38, "1", true, "EDB", "Edukacja dla bezpieczeństwa [wsip]" },
                    { 39, "1", false, "Fizyka", "Fizyka 1 [wsip]" },
                    { 40, "2", false, "Fizyka", "Fizyka 2 [wsip]" },
                    { 41, "3", false, "Fizyka", "Fizyka 3 [wsip]" },
                    { 42, "4/5", false, "Fizyka", "Fizyka 4 [wsip]" },
                    { 43, "1/2", true, "Geografia", "Oblicza geografii 1" },
                    { 44, "3/4", true, "Geografia", "Oblicza geografii 2" },
                    { 45, "1/2", true, "Geografia", "Oblicz geografii karty pracy 1" },
                    { 46, "3/4", true, "Geografia", "Oblicz geografii karty pracy 2" },
                    { 47, "1", true, "Historia", "Historia [wsip] 1" },
                    { 48, "2", true, "Historia", "Historia [wsip] 2" },
                    { 49, "3", true, "Historia", "Historia [wsip] 3" },
                    { 50, "4", true, "Historia", "Historia [wsip] 4" },
                    { 51, "2", true, "HiT", "Historia i teraźniejszość [wsip] 1" },
                    { 52, "3", true, "HiT", "Historia i teraźniejszość [wsip] 2" },
                    { 53, "1/2", true, "Informatyka", "Informatyka [operon]" },
                    { 54, "3/4", true, "Informatyka", "Informatyka dla szkół ponadgimnazjalnych [Migra]" },
                    { 55, "1/2", false, "Informatyka", "Informatyka [operon]" },
                    { 56, "3/4", false, "Informatyka", "Informatyka dla szkół ponadgimnazjalnych [Migra]" },
                    { 57, "1/2", true, "Matematyka", "NOWA MATeMAtyka 1" },
                    { 58, "2/3", true, "Matematyka", "NOWA MATeMAtyka 2" },
                    { 59, "3/4", true, "Matematyka", "NOWA MATeMAtyka 3" },
                    { 60, "4/5", true, "Matematyka", "NOWA MATeMAtyka 4" },
                    { 61, "1/2", false, "Matematyka", "NOWA MATeMAtyka 1" },
                    { 62, "2/3", false, "Matematyka", "NOWA MATeMAtyka 2" },
                    { 63, "3/4", false, "Matematyka", "NOWA MATeMAtyka 3" },
                    { 64, "4/5", false, "Matematyka", "NOWA MATeMAtyka 4" },
                    { 65, "2/3", true, "Podstawy przedsiębiorczości", "Krok w przedsiębiorczość" },
                    { 66, "1", true, "Biznes i zarządzanie", "Krok w biznes i zarządzanie 1" },
                    { 67, "2", true, "Biznes i zarządzanie", "Krok w biznes i zarządzanie 2" },
                    { 68, "1", true, "Plastyka", "Spotkania ze sztuką 1" },
                    { 69, "4", true, "WOS", "W centrum uwagi 1" },
                    { 70, "5", true, "WOS", "W centrum uwagi 2" },
                    { 71, "3/4", false, "Angielski zawodowy", "Electronics" },
                    { 72, "3/4", false, "Angielski zawodowy", "Electrician" },
                    { 73, "3/4", false, "Angielski zawodowy", "Software engineering" },
                    { 74, "3/4", false, "Angielski zawodowy", "Computing" },
                    { 75, "3/4", false, "Angielski zawodowy", "Mechanical engineering" },
                    { 76, "3/4", false, "Angielski zawodowy", "Mechanics" },
                    { 77, "3/4", false, "Angielski zawodowy", "Enviromental Science" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 77);
        }
    }
}
