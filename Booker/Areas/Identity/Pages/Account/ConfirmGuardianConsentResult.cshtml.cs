using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ConfirmGuardianConsentResultModel : PageModel
{
    [TempData]
    public string? Message { get; set; }

    [TempData]
    public bool IsSuccess { get; set; }

    public void OnGet()
    {
    }
}