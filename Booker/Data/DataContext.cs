using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Booker.Data
{
    public class DataContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; } // added
        public DbSet<ChatThread> ChatThreads { get; set; } // added

        // C# doesn't support static local variables in methods, so we have to use a field instead
        private static IEnumerator<int> bookIdGenerator = GenerateAscendingIntegers().GetEnumerator();

        private record BookGrade(int BookId, int GradeId);
        private static List<BookGrade> bookGrades = [
            // other book static data
            new BookGrade(BookId: -1, GradeId: 1),
            new BookGrade(BookId: -1, GradeId: 2),
            new BookGrade(BookId: -1, GradeId: 3),
            new BookGrade(BookId: -1, GradeId: 4),
            new BookGrade(BookId: -1, GradeId: 5)
        ];


        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
           
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subject>().HasData(SeedData.Subjects);

            modelBuilder.Entity<Grade>().HasData(SeedData.Grades);

            modelBuilder.Entity<Level>().HasData(SeedData.Levels);

            modelBuilder.Entity<Book>(b =>
            {
                b.HasData(SeedData.Books);
                b.HasMany(b => b.Grades).WithMany(g => g.Books)
                    .UsingEntity("BookGrades",
                    g => g.HasOne(typeof(Grade)).WithMany().HasForeignKey("GradeId").HasPrincipalKey(nameof(Grade.Id)),
                    b => b.HasOne(typeof(Book)).WithMany().HasForeignKey("BookId").HasPrincipalKey(nameof(Book.Id)),
                    bg =>
                    {
                        bg.HasKey("BookId", "GradeId");
                        bg.HasData(bookGrades);
                    });
            });

            modelBuilder.Entity<User>(u =>
            {
                u.HasMany(u => u.Items).WithOne(i => i.User);
                u.HasMany(u => u.Favorites).WithMany()
                    .UsingEntity("UserFavorites",
                    i => i.HasOne(typeof(Item)).WithMany().HasForeignKey("ItemId").HasPrincipalKey(nameof(Item.Id)),
                    u => u.HasOne(typeof(User)).WithMany().HasForeignKey("UserId").HasPrincipalKey(nameof(User.Id)).OnDelete(DeleteBehavior.Restrict), // to avoid cycles
                    uf =>
                    {
                        uf.HasKey("UserId", "ItemId");
                    });
            });

            modelBuilder.Entity<ChatMessage>(cm =>
            {
                cm.HasIndex(c => new { c.DealId, c.CreatedUtc });
                cm.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ChatThread>(ct =>
            {
                ct.HasIndex(t => t.ChannelId).IsUnique();
                ct.HasIndex(t => new { t.UserAId, t.UserBId });
            });
        }

        public static IEnumerable<int> GenerateAscendingIntegers(int start = 1, int end = 1000)
        {
            for (int i = start; i < end; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<int> GenerateEndlessLoop(int start = 1, int end = 1000)
        {
            while (true)
            {
                for (int i = start; i < end; i++)
                {
                    yield return i;
                }
            }
        }

        private static int GetNextId(IEnumerator<int> idGenerator)
        {
            if (!idGenerator.MoveNext())
                throw new System.InvalidOperationException("Not enough IDs in the generator.");
            return idGenerator.Current;
        }

        public static Book CreateBook(string title, int subjectId, int levelId, List<int> grades)
        {
            var id = GetNextId(bookIdGenerator);
            foreach (var grade in grades)
            {
                bookGrades.Add(new BookGrade(BookId: id, GradeId: grade));
            }

            return new Book
            {
                Id = id,
                Title = title,
                Grades = null!, // Grades will be set by the database
                SubjectId = subjectId,
                Subject = null!, // Subject will be set by the database
                LevelId = levelId,
                Level = null!, // Level will be set by the database
            };
        }
    }
}
