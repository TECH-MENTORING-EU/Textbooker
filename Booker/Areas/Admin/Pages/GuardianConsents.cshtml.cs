using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Booker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Booker.Areas.Admin.Pages;

[Authorize(Policy = "AdminHidden")]
public class GuardianConsentsModel : PageModel
{
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;

    public const int PageSize = 50;

    public GuardianConsentsModel(DataContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<GuardianConsentView> Consents { get; set; } = [];
    public List<PaginationLink> PaginationLinks { get; set; } = [];
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    [FromQuery]
    public string? ChildUserName { get; set; }

    [FromQuery]
    public string? GuardianEmail { get; set; }

    [FromQuery]
    [DataType(DataType.Date)]
    public DateTime? From { get; set; }

    [FromQuery]
    [DataType(DataType.Date)]
    public DateTime? To { get; set; }

    [FromQuery]
    public string? Status { get; set; }  // null, "pending", "confirmed"

    [FromQuery]
    public int PageNumber { get; set; } = 0;

    public async Task<IActionResult> OnGetAsync()
    {
        PageNumber = Math.Max(0, PageNumber);

        var query = _context.GuardianConsents
            .AsNoTracking()
            .Include(gc => gc.User)
            .AsQueryable();

        // Filters
        if (!string.IsNullOrWhiteSpace(ChildUserName))
        {
            query = query.Where(gc => gc.User.UserName!.Contains(ChildUserName));
        }

        if (!string.IsNullOrWhiteSpace(GuardianEmail))
        {
            query = query.Where(gc => gc.GuardianEmail != null && gc.GuardianEmail.Contains(GuardianEmail));
        }

        if (From.HasValue)
        {
            query = query.Where(gc => gc.RequestedAtUtc >= From.Value.Date);
        }

        if (To.HasValue)
        {
            query = query.Where(gc => gc.RequestedAtUtc < To.Value.Date.AddDays(1));
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(Status))
        {
            if (Status == "pending")
                query = query.Where(gc => gc.ConfirmedAtUtc == null);
            else if (Status == "confirmed")
                query = query.Where(gc => gc.ConfirmedAtUtc != null);
        }

        TotalCount = await query.CountAsync();
        PageNumber = Math.Min(PageNumber, Math.Max(0, TotalPages - 1));
        PaginationLinks = BuildPaginationLinks();

        // Build DTO view
        var consents = await query
            .OrderByDescending(gc => gc.RequestedAtUtc)
            .Skip(PageNumber * PageSize)
            .Take(PageSize)
            .ToListAsync();

        Consents = consents.Select(gc => new GuardianConsentView
        {
            Id = gc.Id,
            ChildUserId = gc.UserId,
            ChildUserName = gc.User.UserName,
            ChildEmail = gc.User.Email,
            GuardianEmail = gc.GuardianEmail,
            RequestedAtUtc = gc.RequestedAtUtc,
            ExpiresAtUtc = gc.ExpiresAtUtc,
            ConfirmedAtUtc = gc.ConfirmedAtUtc,
            ConfirmationIpAddress = gc.ConfirmationIpAddress,
            Status = gc.ConfirmedAtUtc.HasValue
                ? gc.GuardianEmail is null ? "Anonymized" : "Confirmed"
                : "Pending"
        }).ToList();

        return Page();
    }

    private List<PaginationLink> BuildPaginationLinks()
    {
        var links = new List<PaginationLink>();

        if (PageNumber > 0)
        {
            links.Add(new PaginationLink(PageNumber - 1, "Poprzednia"));
        }

        if (PageNumber + 1 < TotalPages)
        {
            links.Add(new PaginationLink(PageNumber + 1, "Następna"));
        }

        return links;
    }

    public record PaginationLink(int PageNumber, string Label);

    public class GuardianConsentView
    {
        public int Id { get; set; }
        public int ChildUserId { get; set; }
        public string? ChildUserName { get; set; }
        public string? ChildEmail { get; set; }
        public string? GuardianEmail { get; set; }
        public DateTime RequestedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? ConfirmedAtUtc { get; set; }
        public string? ConfirmationIpAddress { get; set; }
        public string ExpirationLabel => ConfirmedAtUtc.HasValue
            ? "Nie wygasa"
            : ExpiresAtUtc.ToString("g");
        public string Status { get; set; } = string.Empty;
        public string StatusLabel => Status switch
        {
            "Anonymized" => "Anonimizowana",
            "Confirmed" => "Potwierdzona",
            _ => "Oczekuje"
        };
        public string StatusPrefix => Status switch
        {
            "Confirmed" => "✓ ",
            "Pending" => "⏳ ",
            _ => string.Empty
        };
        public string StatusStyle => Status switch
        {
            "Confirmed" => "background-color: green;",
            "Pending" => "background-color: orange;",
            _ => string.Empty
        };
    }
}
