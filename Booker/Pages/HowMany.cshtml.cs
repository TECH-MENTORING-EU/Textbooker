using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


namespace Booker.Pages
{
    public class HowManyModel : PageModel
    {
        private readonly DataContext _context;

        public int UserCount { get; set; } = 0;

        public HowManyModel(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int? code)
        {
            if (code != 2137)
            {
                return NotFound();
            }

            UserCount = await _context.Users.CountAsync();

            return Page();
        }
    }
}
