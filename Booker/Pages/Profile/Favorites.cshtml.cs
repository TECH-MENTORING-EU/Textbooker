using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Booker.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Booker.Pages.Profile
{
    public class FavoritesModel : PageModel
    {
        private readonly DataContext _context;
        public FavoritesModel(DataContext context)
        {
            _context = context;
        }

        public record ButtonState(int Id, bool IsFavorite, bool FullSize);
        [FromRoute]
        public int? Id { get; set; }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAddAsync(int itemId, bool fullSize)
        {
            if (Id != null)
            {
                return BadRequest();
            }

            if (User.Identity?.IsAuthenticated != true)
            {
                Response.Headers["HX-Redirect"] = Url.Page("/Account/Login", new { area = "Identity" });
                return StatusCode(403); // Forbidden, but will redirect via HTMX
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                return Forbid();
            }

            var user = await _context.Users
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Forbid();
            }

            var item = await _context.Items.FindAsync(itemId);

            if (item == null)
            {
                return NotFound();
            }

            if (user.Favorites.Any(f => f.Id == itemId))
            {
                return StatusCode(204);
            }

            user.Favorites.Add(item);
            await _context.SaveChangesAsync();
            return Partial("_FavoriteButton", new ButtonState(itemId, true, fullSize));
        }
        public async Task<IActionResult> OnPostRemoveAsync(int itemId, bool fullSize)
        {
            if (Id != null)
            {
                return BadRequest();
            }

            if (User.Identity?.IsAuthenticated != true)
            {
                Response.Headers["HX-Redirect"] = Url.Page("/Account/Login", new { area = "Identity" });
                return StatusCode(403); // Forbidden, but will redirect via HTMX
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                return Forbid();
            }

            var user = await _context.Users
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Forbid();
            }

            var item = await _context.Items.FindAsync(itemId);

            if (item == null)
            {
                return NotFound();
            }

            if (!user.Favorites.Any(f => f.Id == itemId))
            {
                return StatusCode(204);
            }

            user.Favorites.Remove(item);
            await _context.SaveChangesAsync();
            return Partial("_FavoriteButton", new ButtonState(itemId, false, fullSize));
        }
    }
}
