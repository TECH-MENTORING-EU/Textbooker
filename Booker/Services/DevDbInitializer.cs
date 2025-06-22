using Booker.Data;
using Microsoft.EntityFrameworkCore;

namespace Booker.Services
{
    public class DevDbInitializer
    {
        public static async Task Initialize(DataContext context, int itemsCount, int usersCount)
        {
            if (await context.Items.CountAsync() > itemsCount
                && await context.Users.CountAsync() > usersCount)
            {
                return;
            }

            var lastUserId = await context.Users.AnyAsync()
                ? await context.Users.MaxAsync(u => u.Id)
                : 0;

            var users = new List<User>();

            for (int i = 1; i <= usersCount; i++)
            {
                users.Add(
                    new User
                    {
                        Email = "user" + i + "@gmail.com",
                        UserName = "user" + i,
                        School = "Śl.TZN",
                        Photo = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_1280.png"
                    }
                );
            }

            context.AddRange(users);
            
            var books = await context.Books.ToListAsync();
            var rand = new Random();
            var items = new List<Item>();
            for (int i = 0; i < itemsCount; i++)
            {
                items.Add(
                    new Item
                    {
                        Book = books[rand.Next(books.Count)],
                        User = users[rand.Next(users.Count)],
                        Price = rand.Next(140, 600) / 7M,
                        DateTime = DateTime.Now.AddDays(-(rand.Next(7*24*60)/(24*60.0))), // random date within the last week
                        Description = "Książka w dobrym stanie, prawie nie używana, nie zalana, rogi delikatnie zagięte, polecam kebab Zahir i pytam czy idziecie na sylwestra do zduniaka.",
                        State = "bardzo dobry",
                        Photo = "https://images.unsplash.com/photo-1517770413964-df8ca61194a6?q=80&w=1770&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
                    }
                );
            }

            items.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));
            context.AddRange(items);

            await context.SaveChangesAsync();
        }
    }
}
