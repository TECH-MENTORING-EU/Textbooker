using System.ComponentModel.DataAnnotations;
using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Booker.Areas.Admin.Pages
{
    // RODO — zadanie 09: prosty podgląd dziennika działań administracyjnych.
    public class AuditLogModel : PageModel
    {
        private readonly DataContext _context;

        public AuditLogModel(DataContext context)
        {
            _context = context;
        }

        public List<AdminActionLog> Entries { get; set; } = [];

        [FromQuery]
        public string? AdminUserName { get; set; }

        [FromQuery]
        [DataType(DataType.Date)]
        public DateTime? From { get; set; }

        [FromQuery]
        [DataType(DataType.Date)]
        public DateTime? To { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var query = _context.AdminActionLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(AdminUserName))
            {
                query = query.Where(a => a.AdminUserName.Contains(AdminUserName));
            }

            if (From.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= From.Value.Date);
            }

            if (To.HasValue)
            {
                query = query.Where(a => a.CreatedAt < To.Value.Date.AddDays(1));
            }

            Entries = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Page();
        }
    }
}
