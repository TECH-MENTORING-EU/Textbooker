using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Booker.Data
{
    public class DataContext : IdentityDbContext<User,IdentityRole<int>,int>
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Subject> Subjects { get; set; }

        // C# doesn't support static local variables in methods, so we have to use a field instead
        private static IEnumerator<int> bookIdGenerator = GenerateAscendingIntegers().GetEnumerator();

        private record BookGrade(int BookId, int GradeId);
        private static List<BookGrade> bookGrades = new List<BookGrade>();


        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
           
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var userIdGenerator = GenerateAscendingIntegers().GetEnumerator();
            var itemIdGenerator = GenerateAscendingIntegers().GetEnumerator();
            var userSequenceGenerator = GenerateEndlessLoop(1, 6).GetEnumerator();
            var rand = new Random();

            modelBuilder.Entity<Subject>().HasData(SeedData.Subjects);

            modelBuilder.Entity<Grade>().HasData(SeedData.Grades);

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



            var users = new List<User>();

            for (int i = 0; i < 5; i++)
            {
                users.Add(
                    new User
                    { 
                        Id = GetNextId(userIdGenerator),
                        Email = "user" + GetCurrentId(userIdGenerator) + "@gmail.com",
                        UserName = "user" + GetCurrentId(userIdGenerator),
                        School = "Śl.TZN",
                        Photo = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_1280.png"
                    }
                );
            }

            modelBuilder.Entity<User>().HasData(users);



            var items = new List<Item>();

            for (int i = 0; i < 50; i++)
            {
                items.Add(
                    new Item
                    {
                        Id = GetNextId(itemIdGenerator),
                        BookId = rand.Next(1, SeedData.Books.Count),
                        Book = null!, // Book will be set by the database
                        UserId = GetNextId(userSequenceGenerator),
                        User = null!, // User will be set by the database
                        Price = rand.Next(140, 600) / 7M,
                        DateTime = DateTime.Now,
                        Description = "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.",
                        State = "bardzo dobry",
                        Photo = "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
                    }
                );
            }

            modelBuilder.Entity<Item>().HasData(items);
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

        private static int GetCurrentId(IEnumerator<int> idGenerator)
        {
            return idGenerator.Current;
        }

        public static Book CreateBook(string title, int subjectId, bool level, List<int> grades)
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
                Level = level
            };
        }
    }
}
