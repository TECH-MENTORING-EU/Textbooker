namespace Booker.Data
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using static DataContext;
    public static class SeedData
    {
        /// <summary>
        /// Development-only schools seeded at runtime in Development environment.
        /// </summary>
        private readonly static List<School> DevelopmentSchools =
        [
            new School { Id = 1, Name = "Hogwort", EmailDomain = "hogwart.edu.pl", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new School { Id = 2, Name = "Technikum Pod Patronatem Przypadkowego Gościa z Discorda", EmailDomain = "technikum-discord.edu.pl", CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new School { Id = 3, Name = "Uniwersytet Bestroskiego Zycia W Obliczu Zagłady im. Augusta III Sasa", EmailDomain = "ubz-august.edu.pl", CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) }
        ];

        private const string DevelopmentPassword = "TestPass123!";

        private sealed record DevelopmentAccount(string UserName, string Email);

        private readonly static List<DevelopmentAccount> DevelopmentAccounts =
        [
            new DevelopmentAccount("u1", "u1@hogwart.edu.pl"),
            new DevelopmentAccount("u2", "u2@technikum-discord.edu.pl"),
            new DevelopmentAccount("u3", "u3@ubz-august.edu.pl"),
            new DevelopmentAccount("u4", "u4@hogwart.edu.pl"),
            new DevelopmentAccount("u5", "u5@technikum-discord.edu.pl"),
            new DevelopmentAccount("u6", "u6@ubz-august.edu.pl"),
            new DevelopmentAccount("a1", "a1@hogwart.edu.pl")
        ];

        public static async Task InitializeDevelopmentDataAsync(DataContext context, UserManager<User> userManager, int itemsCount, int usersCount)
        {
            await EnsureSeedSchoolsAsync(context);

            if (await context.Users.AnyAsync())
            {
                return;
            }

            await EnsureCredentialUsersAsync(context, userManager);

            var books = await context.Books.ToListAsync();
            if (books.Count == 0)
            {
                return;
            }

            var school1Id = await GetSchoolIdByDomainAsync(context, "hogwart.edu.pl");
            if (school1Id.HasValue)
            {
                await SeedRandomUsersAndItemsForSchoolAsync(context, books, schoolId: school1Id.Value, randomPrefix: "r1", usersCount, itemsCount);
            }

            var school2Id = await GetSchoolIdByDomainAsync(context, "technikum-discord.edu.pl");
            if (school2Id.HasValue)
            {
                await SeedRandomUsersAndItemsForSchoolAsync(context, books, schoolId: school2Id.Value, randomPrefix: "r2", usersCount, itemsCount);
            }

            var school3Id = await GetSchoolIdByDomainAsync(context, "ubz-august.edu.pl");
            if (school3Id.HasValue)
            {
                await SeedRandomUsersAndItemsForSchoolAsync(context, books, schoolId: school3Id.Value, randomPrefix: "r3", usersCount, itemsCount);
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureSeedSchoolsAsync(DataContext context)
        {
            var hasChanges = false;

            foreach (var seedSchool in DevelopmentSchools)
            {
                var seedDomain = seedSchool.EmailDomain?.Trim().ToLower();
                var school = await context.Schools.SingleOrDefaultAsync(s =>
                    s.EmailDomain != null &&
                    s.EmailDomain.Trim().ToLower() == seedDomain);

                school ??= await context.Schools.SingleOrDefaultAsync(s => s.Id == seedSchool.Id);
                if (school is null)
                {
                    context.Schools.Add(new School
                    {
                        Name = seedSchool.Name,
                        EmailDomain = seedSchool.EmailDomain,
                        IsActive = true,
                        CreatedAt = seedSchool.CreatedAt,
                        DeactivatedAt = null
                    });

                    hasChanges = true;
                    continue;
                }

                if (!string.Equals(school.Name, seedSchool.Name, StringComparison.Ordinal))
                {
                    school.Name = seedSchool.Name;
                    hasChanges = true;
                }

                if (!string.Equals(school.EmailDomain, seedSchool.EmailDomain, StringComparison.OrdinalIgnoreCase))
                {
                    school.EmailDomain = seedSchool.EmailDomain;
                    hasChanges = true;
                }

                if (!school.IsActive)
                {
                    school.IsActive = true;
                    hasChanges = true;
                }

                if (school.DeactivatedAt is not null)
                {
                    school.DeactivatedAt = null;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await context.SaveChangesAsync();
            }
        }

        private static async Task EnsureCredentialUsersAsync(DataContext context, UserManager<User> userManager)
        {
            foreach (var account in DevelopmentAccounts)
            {
                var schoolId = await GetSchoolIdByDomainFromEmailAsync(context, account.Email);
                var user = await userManager.FindByNameAsync(account.UserName);
                if (user is null)
                {
                    var newUser = new User
                    {
                        UserName = account.UserName,
                        Email = account.Email,
                        SchoolId = schoolId,
                        EmailConfirmed = true,
                        Photo = "/img/default-profile-picture.jpg"
                    };

                    var createResult = await userManager.CreateAsync(newUser, DevelopmentPassword);
                    if (!createResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to create seed user '{account.UserName}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }

                    continue;
                }

                var needsUpdate = false;
                if (!string.Equals(user.Email, account.Email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = account.Email;
                    needsUpdate = true;
                }

                if (user.SchoolId != schoolId)
                {
                    user.SchoolId = schoolId;
                    needsUpdate = true;
                }

                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    needsUpdate = true;
                }

                if (string.IsNullOrWhiteSpace(user.Photo))
                {
                    user.Photo = "/img/default-profile-picture.jpg";
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    var updateResult = await userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to update seed user '{account.UserName}': {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                    }
                }

                if (!await userManager.HasPasswordAsync(user))
                {
                    var addPasswordResult = await userManager.AddPasswordAsync(user, DevelopmentPassword);
                    if (!addPasswordResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to set password for seed user '{account.UserName}': {string.Join(", ", addPasswordResult.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private static async Task<int?> GetSchoolIdByDomainFromEmailAsync(DataContext context, string email)
        {
            var atIndex = email.LastIndexOf('@');
            if (atIndex < 0 || atIndex == email.Length - 1)
            {
                return null;
            }

            var domain = email[(atIndex + 1)..].Trim().ToLowerInvariant();
            return await GetSchoolIdByDomainAsync(context, domain);
        }

        private static async Task<int?> GetSchoolIdByDomainAsync(DataContext context, string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                return null;
            }

            var normalizedDomain = domain.Trim().ToLowerInvariant();

            var schools = await context.Schools
                .Where(s => s.EmailDomain != null)
                .Select(s => new { s.Id, s.EmailDomain })
                .ToListAsync();

            return schools
                .Where(s => s.EmailDomain!
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => d.Trim().ToLowerInvariant())
                    .Contains(normalizedDomain))
                .Select(s => (int?)s.Id)
                .FirstOrDefault();
        }

        private static async Task SeedRandomUsersAndItemsForSchoolAsync(
            DataContext context,
            List<Book> books,
            int schoolId,
            string randomPrefix,
            int usersCount,
            int itemsCount)
        {
            var userPrefix = randomPrefix + "_";

            if (await context.Users.AnyAsync(u => u.SchoolId == schoolId && u.UserName != null && u.UserName.StartsWith(userPrefix)))
            {
                return;
            }

            var randomUsersInSchool = await context.Users
                .Where(u => u.SchoolId == schoolId && u.UserName != null && u.UserName.StartsWith(userPrefix))
                .ToListAsync();

            var missingUsersCount = Math.Max(0, usersCount - randomUsersInSchool.Count);
            if (missingUsersCount > 0)
            {
                var newUsers = Enumerable.Range(0, missingUsersCount)
                    .Select(_ =>
                    {
                        var suffix = Guid.NewGuid().ToString("N")[..10];
                        var userName = userPrefix + suffix;
                        return new User
                        {
                            UserName = userName,
                            Email = userName + "@seed.local",
                            SchoolId = schoolId,
                            EmailConfirmed = true,
                            Photo = "/img/default-profile-picture.jpg"
                        };
                    })
                    .ToList();

                context.Users.AddRange(newUsers);
                randomUsersInSchool.AddRange(newUsers);
            }

            if (randomUsersInSchool.Count == 0)
            {
                return;
            }

            var currentItemsCount = await context.Items.CountAsync(i =>
                i.User.SchoolId == schoolId &&
                i.User.UserName != null &&
                i.User.UserName.StartsWith(userPrefix));

            var missingItemsCount = Math.Max(0, itemsCount - currentItemsCount);
            if (missingItemsCount == 0)
            {
                return;
            }

            var items = Enumerable.Range(0, missingItemsCount)
                .Select(_ =>
                {
                    var user = randomUsersInSchool[Random.Shared.Next(randomUsersInSchool.Count)];
                    var book = books[Random.Shared.Next(books.Count)];
                    return new Item
                    {
                        Book = book,
                        User = user,
                        Price = Random.Shared.Next(140, 600) / 7M,
                        CreatedAt = DateTime.Now.AddDays(-(Random.Shared.Next(7 * 24 * 60) / (24 * 60.0))),
                        Description = "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.",
                        State = "bardzo dobry",
                        Photo = "/img/default-book.svg"
                    };
                })
                .OrderBy(i => i.CreatedAt)
                .ToList();

            context.Items.AddRange(items);
        }

        public readonly static List<Grade> Grades =
        [
            new Grade { Id = 1, GradeNumber = "1" },
            new Grade { Id = 2, GradeNumber = "2" },
            new Grade { Id = 3, GradeNumber = "3" },
            new Grade { Id = 4, GradeNumber = "4" },
            new Grade { Id = 5, GradeNumber = "5" }
        ];

        // Hard-coded IDs, should it be like that?
        public readonly static List<Subject> Subjects =
        [
            new Subject { Id = -1, Name = "Brak" },
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
            new Subject { Id = 17, Name = "Język angielski zawodowy" },
            new Subject { Id = 18, Name = "Edukacja obywatelska" }
        ];

        public readonly static List<Level> Levels =
        [
            new Level { Id = -1, Name = "Brak" },
            new Level { Id = 1, Name = "Podstawa" },
            new Level { Id = 2, Name = "Rozszerzenie" },
            new Level { Id = 3, Name = "Podstawa+Rozszerzenie" },
            new Level { Id = 4, Name = "Dwujęzyczny" }
        ];

        public readonly static List<Book> Books =
        [
            // Inna książka
            new() { Id = -1, Title = "Inna", SubjectId = -1, Subject = null!, LevelId = -1, Level = null!, Grades = null! },

            // Polski
            CreateBook(title: "Ponad słowami 1 cz. 1", subjectId: 1, levelId: 3, grades: new() { 1 }),
            CreateBook(title: "Ponad słowami 1 cz. 2", subjectId: 1, levelId: 3, grades: new() { 1 }),
            CreateBook(title: "Ponad słowami 2 cz. 1", subjectId: 1, levelId: 3, grades: new() { 2 }),
            CreateBook(title: "Ponad słowami 2 cz. 2", subjectId: 1, levelId: 3, grades: new() { 2,3 }),
            CreateBook(title: "Ponad słowami 3 cz. 1", subjectId: 1, levelId: 3, grades: new() { 3 }),
            CreateBook(title: "Ponad słowami 3 cz. 2", subjectId: 1, levelId: 3, grades: new() { 3,4 }),
            CreateBook(title: "Ponad słowami 4", subjectId: 1, levelId: 3, grades: new() { 4,5 }),

            // Język angielski
            CreateBook(title: "Focus 2 Podręcznik", subjectId: 2, levelId: 3, grades: new() { 1,2,3 }),
            CreateBook(title: "Focus 3 Podręcznik", subjectId: 2, levelId: 3, grades: new() { 1,2,3 }),
            CreateBook(title: "Focus 4 Podręcznik", subjectId: 2, levelId: 3, grades: new() { 1,2,3,4 }),
            CreateBook(title: "Focus 5 Podręcznik", subjectId: 2, levelId: 3, grades: new() { 3,4,5 }),
            CreateBook(title: "Focus 2 Ćwiczenia", subjectId: 2, levelId: 3, grades: new() { 1,2,3 }),
            CreateBook(title: "Focus 3 Ćwiczenia", subjectId: 2, levelId: 3, grades: new() { 1,2,3 }),
            CreateBook(title: "Focus 4 Ćwiczenia", subjectId: 2, levelId: 3, grades: new() { 1,2,3,4 }),
            CreateBook(title: "Focus 5 Ćwiczenia", subjectId: 2, levelId: 3, grades: new() { 3,4,5 }),
            CreateBook(title: "My matura perspectives [nowa era]", subjectId: 2, levelId: 3, grades: new() { 4,5 }),
            CreateBook(title: "Repetytorium [Macmillan]", subjectId: 2, levelId: 3, grades: new() { 5 }),
            CreateBook(title: "Repetytorium maturzysty [Oxford]", subjectId: 2, levelId: 2, grades: new() { 5 }),
            CreateBook(title: "Repetytorium maturzysty [Cambridge, PWN]", subjectId: 2, levelId: 3, grades: new() { 5 }),

            // Język Niemiecki
            CreateBook(title: "Welttour Deutsch 1", subjectId: 3, levelId: 3, grades: new() { 1 }),
            CreateBook(title: "Welttour Deutsch 2", subjectId: 3, levelId: 3, grades: new() { 1,2 }),
            CreateBook(title: "Welttour Deutsch 3", subjectId: 3, levelId: 3, grades: new() { 3 }),
            CreateBook(title: "Welttour Deutsch 4", subjectId: 3, levelId: 3, grades: new() { 4,5 }),
            CreateBook(title: "Effekt 1", subjectId: 3, levelId: 3, grades: new() { 1,2 }),
            CreateBook(title: "Effekt 2", subjectId: 3, levelId: 3, grades: new() { 2,3 }),
            CreateBook(title: "Effekt 3", subjectId: 3, levelId: 3, grades: new() { 3,4 }),
            CreateBook(title: "Effekt 4", subjectId: 3, levelId: 3, grades: new() { 4,5 }),

            // Biologia
            CreateBook(title: "Biologia na czasie 1", subjectId: 4, levelId: 1, grades: new() { 1 }),
            CreateBook(title: "Biologia na czasie 2", subjectId: 4, levelId: 1, grades: new() { 2,3 }),
            CreateBook(title: "Biologia na czasie 3", subjectId: 4, levelId: 1, grades: new() { 3,4 }),
            CreateBook(title: "Biologia na czasie 1", subjectId: 4, levelId: 2, grades: new() { 1 }),
            CreateBook(title: "Biologia na czasie 2", subjectId: 4, levelId: 2, grades: new() { 2 }),
            CreateBook(title: "Biologia na czasie 3", subjectId: 4, levelId: 2, grades: new() { 3 }),
            CreateBook(title: "Biologia na czasie 4", subjectId: 4, levelId: 2, grades: new() { 4 }),

            // Chemia
            CreateBook(title: "To jest chemia 1", subjectId: 5, levelId: 1, grades: new() { 1,2,3 }),
            CreateBook(title: "To jest chemia 2", subjectId: 5, levelId: 1, grades: new() { 2,3,4 }),
            CreateBook(title: "To jest chemia 1", subjectId: 5, levelId: 2, grades: new() { 1,2,3 }),
            CreateBook(title: "To jest chemia 2", subjectId: 5, levelId: 2, grades: new() { 2,3,4,5 }),

            // EDB
            CreateBook(title: "Edukacja dla bezpieczeństwa [wsip]", subjectId: 6, levelId: 1, grades: new() { 1 }),

            // Fizyka
            CreateBook(title: "Fizyka 1 [wsip]", subjectId: 7, levelId: 2, grades: new() { 1 }),
            CreateBook(title: "Fizyka 2 [wsip]", subjectId: 7, levelId: 2, grades: new() { 2 }),
            CreateBook(title: "Fizyka 3 [wsip]", subjectId: 7, levelId: 2, grades: new() { 3 }),
            CreateBook(title: "Fizyka 4 [wsip]", subjectId: 7, levelId: 2, grades: new() { 4,5 }),
            CreateBook(title: "Fizyka 1 [wsip]", subjectId: 7, levelId: 1, grades: new() { 1 }),
            CreateBook(title: "Fizyka 2 [wsip]", subjectId: 7, levelId: 1, grades: new() { 2 }),
            CreateBook(title: "Fizyka 3 [wsip]", subjectId: 7, levelId: 1, grades: new() { 3 }),
            CreateBook(title: "Fizyka 4 [wsip]", subjectId: 7, levelId: 1, grades: new() { 4,5 }),

            // Geografia
            CreateBook(title: "Oblicza geografii 1", subjectId: 8, levelId: 1, grades: new() { 1,2 }),
            CreateBook(title: "Oblicza geografii 2", subjectId: 8, levelId: 1, grades: new() { 2,3,4 }),
            CreateBook(title: "Oblicza geografii karty pracy 1", subjectId: 8, levelId: 1, grades: new() { 1,2 }),
            CreateBook(title: "Oblicza geografii karty pracy 2", subjectId: 8, levelId: 1, grades: new() { 2,3,4 }),

            // Historia
            CreateBook(title: "Historia [wsip] 1", subjectId: 9, levelId: 1, grades: new() { 1 }),
            CreateBook(title: "Historia [wsip] 2", subjectId: 9, levelId: 1, grades: new() { 2 }),
            CreateBook(title: "Historia [wsip] 3", subjectId: 9, levelId: 1, grades: new() { 3 }),
            CreateBook(title: "Historia [wsip] 4", subjectId: 9, levelId: 1, grades: new() { 4,5 }),

            // HiT
            CreateBook(title: "Historia i teraźniejszość [wsip] 1", subjectId: 10, levelId: 1, grades: new() { 2 }),
            CreateBook(title: "Historia i teraźniejszość [wsip] 2", subjectId: 10, levelId: 1, grades: new() { 3 }),

            // Informatyka
            CreateBook(title: "Informatyka [operon]", subjectId: 11, levelId: 1, grades: new() { 1,2 }),
            CreateBook(title: "Informatyka dla szkół ponadgimnazjalnych [Migra]", subjectId: 11, levelId: 1, grades: new() { 2,3,4 }),
            CreateBook(title: "Informatyka [operon]", subjectId: 11, levelId: 2, grades: new() { 1,2 }),
            CreateBook(title: "Informatyka dla szkół ponadgimnazjalnych [Migra]", subjectId: 11, levelId: 2, grades: new() { 2,3,4 }),

            // Matematyka
            CreateBook(title: "NOWA MATeMAtyka 1", subjectId: 12, levelId: 1, grades: new() { 1,2 }),
            CreateBook(title: "NOWA MATeMAtyka 2", subjectId: 12, levelId: 1, grades: new() { 2,3 }),
            CreateBook(title: "NOWA MATeMAtyka 3", subjectId: 12, levelId: 1, grades: new() { 3,4 }),
            CreateBook(title: "NOWA MATeMAtyka 4", subjectId: 12, levelId: 1, grades: new() { 4,5 }),
            CreateBook(title: "NOWA MATeMAtyka 1", subjectId: 12, levelId: 3, grades: new() { 1,2 }),
            CreateBook(title: "NOWA MATeMAtyka 2", subjectId: 12, levelId: 3, grades: new() { 2,3 }),
            CreateBook(title: "NOWA MATeMAtyka 3", subjectId: 12, levelId: 3, grades: new() { 3,4 }),
            CreateBook(title: "NOWA MATeMAtyka 4", subjectId: 12, levelId: 3, grades: new() { 4,5 }),

            // Podstawy przedsiębiorczości
            CreateBook(title: "Krok w przedsiębiorczość", subjectId: 13, levelId: 1, grades: new() { 2 }),

            // Biznes i zarządzanie
            CreateBook(title: "Krok w biznes i zarządzanie 1", subjectId: 14, levelId: 1, grades: new() { 1 }),
            CreateBook(title: "Krok w biznes i zarządzanie 2", subjectId: 14, levelId: 1, grades: new() { 2 }),

            // Plastyka
            CreateBook(title: "Spotkania ze sztuką 1", subjectId: 15, levelId: 1, grades: new() { 1 }),

            // Edukacja obywatelska
            CreateBook(title: "Masz wpływ 1", subjectId: 18, levelId: 1, grades: new() { 1,2 }),

            // WOS
            CreateBook(title: "W centrum uwagi 1", subjectId: 16, levelId: 1, grades: new() { 4,5 }),
            CreateBook(title: "W centrum uwagi 2", subjectId: 16, levelId: 1, grades: new() { 4,5 }),

            // Angielski zawodowy
            CreateBook(title: "Electronics", subjectId: 17, levelId: -1, grades: new() { 3,4 }),
            CreateBook(title: "Electrician", subjectId: 17, levelId: -1, grades: new() { 3,4 }),
            CreateBook(title: "Software engineering", subjectId: 17, levelId: -1, grades: new() { 3,4 }),
            CreateBook(title: "Computing", subjectId: 17, levelId: -1, grades: new() { 3,4 }),
            CreateBook(title: "Mechanical engineering", subjectId: 17, levelId: -1, grades: new() { 3,4 }),
            CreateBook(title: "Mechanics", subjectId: 17, levelId: -1, grades: new() { 3,4 }),
            CreateBook(title: "Environmental Science", subjectId: 17, levelId: -1, grades: new() { 3,4 }),
            CreateBook(title: "IT [english for IT]", subjectId: 17, levelId: -1, grades: new() { 3,4 }),

            CreateBook(title: "Informatyka w praktyce", subjectId: 11, levelId: 2, grades: new() { 3 })
        ];
    }
}
