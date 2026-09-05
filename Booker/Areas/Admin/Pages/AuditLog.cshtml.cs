using System.ComponentModel.DataAnnotations;
using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Booker.Areas.Admin.Pages
{
    // RODO - task 09: simple read-only view of the administrative action log.
    public class AuditLogModel : PageModel
    {
        private readonly DataContext _context;

        // This table is append-only and grows with every administrative action (unlike the
        // user list), so it is paginated server-side instead of being materialized in full.
        public const int PageSize = 50;

        public AuditLogModel(DataContext context)
        {
            _context = context;
        }

        public List<AdminActionLog> Entries { get; set; } = [];

        public int TotalCount { get; set; }

        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        [FromQuery]
        public string? AdminUserName { get; set; }

        [FromQuery]
        [DataType(DataType.Date)]
        public DateTime? From { get; set; }

        [FromQuery]
        [DataType(DataType.Date)]
        public DateTime? To { get; set; }

        [FromQuery]
        public int PageNumber { get; set; } = 0;

        public async Task<IActionResult> OnGetAsync()
        {
            PageNumber = Math.Max(0, PageNumber);

            var query = _context.AdminActionLogs.AsNoTracking().AsQueryable();

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

            TotalCount = await query.CountAsync();

            Entries = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip(PageNumber * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }
    }
}
