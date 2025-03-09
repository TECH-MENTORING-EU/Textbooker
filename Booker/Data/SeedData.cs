namespace Booker.Data
{
    using Microsoft.EntityFrameworkCore;
    using static DataContext;
    public static class SeedData
    {
        public static List<Grade> Grades = new List<Grade>
        {
            new Grade { Id = 1, GradeNumber = "1" },
            new Grade { Id = 2, GradeNumber = "2" },
            new Grade { Id = 3, GradeNumber = "3" },
            new Grade { Id = 4, GradeNumber = "4" },
            new Grade { Id = 5, GradeNumber = "5" }
        };

        // Hard-coded IDs, should it be like that?
        public static List<Subject> Subjects = new List<Subject>
        {
            new Subject { Id = 1, Name = "Język polski" },
            new Subject { Id = 2, Name = "Język angielski" },
            new Subject { Id = 3, Name = "Język niemiecki" },
            new Subject { Id = 4, Name = "Biologia" },
            new Subject { Id = 5, Name = "Chemia" },
            new Subject { Id = 6, Name = "EDB" },
            new Subject { Id = 7, Name = "Fizyka" },
            new Subject { Id = 8, Name = "Geografia" },
            new Subject { Id = 9, Name = "Historia" },
            new Subject { Id = 10, Name = "Historia i teraźniejszość" },
            new Subject { Id = 11, Name = "Informatyka" },
            new Subject { Id = 12, Name = "Matematyka" },
            new Subject { Id = 13, Name = "Podstawy przedsiębiorczości" },
            new Subject { Id = 14, Name = "Biznes i zarządzanie" },
            new Subject { Id = 15, Name = "Plastyka" },
            new Subject { Id = 16, Name = "WOS" },
            new Subject { Id = 17, Name = "Język angielski zawodowy" }
        };

        public static List<Book> Books = new List<Book>
        {
            // Polski
            CreateBook(title: "Ponad słowami 1 cz. 1", subjectId: 1, level: false, grades: new() { 1 }),
            CreateBook(title: "Ponad słowami 1 cz. 2", subjectId: 1, level: false, grades: new() { 1 }),
            CreateBook(title: "Ponad słowami 2 cz. 1", subjectId: 1, level: false, grades: new() { 2 }),
            CreateBook(title: "Ponad słowami 2 cz. 2", subjectId: 1, level: false, grades: new() { 2 }),
            CreateBook(title: "Ponad słowami 3 cz. 1", subjectId: 1, level: false, grades: new() { 3 }),
            CreateBook(title: "Ponad słowami 3 cz. 2", subjectId: 1, level: false, grades: new() { 4 }),
            CreateBook(title: "Ponad słowami 4", subjectId: 1, level: false, grades: new() { 5 }),

            // Język angielski
            CreateBook(title: "Focus 2 Podręcznik", subjectId: 2, level: false, grades: new() { 1 }),
            CreateBook(title: "Focus 3 Podręcznik", subjectId: 2, level: false, grades: new() { 1 }),
            CreateBook(title: "Focus 4 Podręcznik", subjectId: 2, level: false, grades: new() { 2 }),
            CreateBook(title: "Focus 5 Podręcznik", subjectId: 2, level: false, grades: new() { 2 }),
            CreateBook(title: "Focus 2 Ćwiczenia", subjectId: 2, level: false, grades: new() { 3 }),
            CreateBook(title: "Focus 3 Ćwiczenia", subjectId: 2, level: false, grades: new() { 3 }),
            CreateBook(title: "Focus 4 Ćwiczenia", subjectId: 2, level: false, grades: new() { 4 }),
            CreateBook(title: "Focus 5 Ćwiczenia", subjectId: 2, level: false, grades: new() { 4 }),
            CreateBook(title: "My matura perspectives [nowa era]", subjectId: 2, level: false, grades: new() { 5 }),
            CreateBook(title: "Repetytorium [Macmillan]", subjectId: 2, level: false, grades: new() { 5 }),
            CreateBook(title: "Repetytorium maturzysty [Oxford]", subjectId: 2, level: true, grades: new() { 5 }),
            CreateBook(title: "Repetytorium maturzysty [Cambridge, PWN]", subjectId: 2, level: false, grades: new() { 5 }),

            // Język Niemiecki
            CreateBook(title: "Welttour Deutsch 1", subjectId: 3, level: false, grades: new() { 1 }),
            CreateBook(title: "Welttour Deutsch 2", subjectId: 3, level: false, grades: new() { 2 }),
            CreateBook(title: "Welttour Deutsch 3", subjectId: 3, level: false, grades: new() { 3 }),
            CreateBook(title: "Welttour Deutsch 4", subjectId: 3, level: false, grades: new() { 4 }),
            CreateBook(title: "Effekt 1", subjectId: 3, level: false, grades: new() { 1 }),
            CreateBook(title: "Effekt 2", subjectId: 3, level: false, grades: new() { 2 }),
            CreateBook(title: "Effekt 3", subjectId: 3, level: false, grades: new() { 3 }),
            CreateBook(title: "Effekt 4", subjectId: 3, level: false, grades: new() { 4 }),

            // Biologia
            CreateBook(title: "Biologia na czasie 1", subjectId: 4, level: false, grades: new() { 1 }),
            CreateBook(title: "Biologia na czasie 2", subjectId: 4, level: false, grades: new() { 2 }),
            CreateBook(title: "Biologia na czasie 3", subjectId: 4, level: false, grades: new() { 3 }),
            CreateBook(title: "Biologia na czasie 1", subjectId: 4, level: true, grades: new() { 1 }),
            CreateBook(title: "Biologia na czasie 2", subjectId: 4, level: true, grades: new() { 2 }),
            CreateBook(title: "Biologia na czasie 3", subjectId: 4, level: true, grades: new() { 3 }),

            // Chemia
            CreateBook(title: "To jest chemia 1", subjectId: 5, level: false, grades: new() { 1 }),
            CreateBook(title: "To jest chemia 2", subjectId: 5, level: false, grades: new() { 2 }),
            CreateBook(title: "To jest chemia 1", subjectId: 5, level: true, grades: new() { 1 }),
            CreateBook(title: "To jest chemia 2", subjectId: 5, level: true, grades: new() { 2 }),

            // EDB
            CreateBook(title: "Edukacja dla bezpieczeństwa [wsip]", subjectId: 6, level: false, grades: new() { 1 }),

            // Fizyka
            CreateBook(title: "Fizyka 1 [wsip]", subjectId: 7, level: true, grades: new() { 1 }),
            CreateBook(title: "Fizyka 2 [wsip]", subjectId: 7, level: true, grades: new() { 2 }),
            CreateBook(title: "Fizyka 3 [wsip]", subjectId: 7, level: true, grades: new() { 3 }),
            CreateBook(title: "Fizyka 4 [wsip]", subjectId: 7, level: true, grades: new() { 4 }),
            CreateBook(title: "Fizyka 1 [wsip]", subjectId: 7, level: false, grades: new() { 1 }),
            CreateBook(title: "Fizyka 2 [wsip]", subjectId: 7, level: false, grades: new() { 2 }),
            CreateBook(title: "Fizyka 3 [wsip]", subjectId: 7, level: false, grades: new() { 3 }),
            CreateBook(title: "Fizyka 4 [wsip]", subjectId: 7, level: false, grades: new() { 4 }),

            // Geografia
            CreateBook(title: "Oblicza geografii 1", subjectId: 8, level: false, grades: new() { 1 }),
            CreateBook(title: "Oblicza geografii 2", subjectId: 8, level: false, grades: new() { 2 }),
            CreateBook(title: "Oblicz geografii karty pracy 1", subjectId: 8, level: false, grades: new() { 1 }),
            CreateBook(title: "Oblicz geografii karty pracy 2", subjectId: 8, level: false, grades: new() { 2 }),

            // Historia
            CreateBook(title: "Historia [wsip] 1", subjectId: 9, level: false, grades: new() { 1 }),
            CreateBook(title: "Historia [wsip] 2", subjectId: 9, level: false, grades: new() { 2 }),
            CreateBook(title: "Historia [wsip] 3", subjectId: 9, level: false, grades: new() { 3 }),
            CreateBook(title: "Historia [wsip] 4", subjectId: 9, level: false, grades: new() { 4 }),

            // HiT
            CreateBook(title: "Historia i teraźniejszość [wsip] 1", subjectId: 10, level: false, grades: new() { 2 }),
            CreateBook(title: "Historia i teraźniejszość [wsip] 2", subjectId: 10, level: false, grades: new() { 3 }),

            // Informatyka
            CreateBook(title: "Informatyka [operon]", subjectId: 11, level: false, grades: new() { 1 }),
            CreateBook(title: "Informatyka dla szkół ponadgimnazjalnych [Migra]", subjectId: 11, level: false, grades: new() { 2 }),
            CreateBook(title: "Informatyka [operon]", subjectId: 11, level: true, grades: new() { 1 }),
            CreateBook(title: "Informatyka dla szkół ponadgimnazjalnych [Migra]", subjectId: 11, level: true, grades: new() { 2 }),

            // Matematyka
            CreateBook(title: "NOWA MATeMAtyka 1", subjectId: 12, level: false, grades: new() { 1 }),
            CreateBook(title: "NOWA MATeMAtyka 2", subjectId: 12, level: false, grades: new() { 2 }),
            CreateBook(title: "NOWA MATeMAtyka 3", subjectId: 12, level: false, grades: new() { 3 }),
            CreateBook(title: "NOWA MATeMAtyka 4", subjectId: 12, level: false, grades: new() { 4 }),
            CreateBook(title: "NOWA MATeMAtyka 1", subjectId: 12, level: true, grades: new() { 1 }),
            CreateBook(title: "NOWA MATeMAtyka 2", subjectId: 12, level: true, grades: new() { 2 }),
            CreateBook(title: "NOWA MATeMAtyka 3", subjectId: 12, level: true, grades: new() { 3 }),
            CreateBook(title: "NOWA MATeMAtyka 4", subjectId: 12, level: true, grades: new() { 4 }),

            // Podstawy przedsiębiorczości
            CreateBook(title: "Krok w przedsiębiorczość", subjectId: 13, level: false, grades: new() { 2 }),

            // Biznes i zarządzanie
            CreateBook(title: "Krok w biznes i zarządzanie 1", subjectId: 14, level: false, grades: new() { 1 }),
            CreateBook(title: "Krok w biznes i zarządzanie 2", subjectId: 14, level: false, grades: new() { 2 }),

            // Plastyka
            CreateBook(title: "Spotkania ze sztuką 1", subjectId: 15, level: false, grades: new() { 1 }),

            // WOS
            CreateBook(title: "W centrum uwagi 1", subjectId: 16, level: false, grades: new() { 4 }),
            CreateBook(title: "W centrum uwagi 2", subjectId: 16, level: false, grades: new() { 5 }),

            // Angielski zawodowy
            CreateBook(title: "Electronics", subjectId: 17, level: true, grades: new() { 3 }),
            CreateBook(title: "Electrician", subjectId: 17, level: true, grades: new() { 3 }),
            CreateBook(title: "Software engineering", subjectId: 17, level: true, grades: new() { 3 }),
            CreateBook(title: "IT [english for IT]", subjectId: 17, level: false, grades: new() { 3 }),

            // Informatyka
            CreateBook(title: "Informatyka w praktyce", subjectId: 11, level: true, grades: new() { 3 })
        };
    }
}
