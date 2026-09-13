using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Booker.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [EnableRateLimiting("IpRateLimitAllMethods")]
    public class ConfirmGuardianConsentModel : PageModel
    {
        private readonly GuardianConsentService _consentService;
        private readonly ILogger<ConfirmGuardianConsentModel> _logger;
        private readonly GuardianConsentOptions _consentOptions;

        public ConfirmGuardianConsentModel(
            GuardianConsentService consentService,
            ILogger<ConfirmGuardianConsentModel> logger,
            IOptions<GuardianConsentOptions> consentOptions)
        {
            _consentService = consentService;
            _logger = logger;
            _consentOptions = consentOptions.Value;
        }

        public string? Message { get; set; }

        /// <summary>
        /// True when the GET-supplied link is valid and safe to present a confirmation
        /// form for. False for invalid/expired/used links.
        /// </summary>
        public bool CanConfirm { get; set; }

        public string? ChildUserName { get; set; }
        public string? ChildEmail { get; set; }

        public int TokenExpirationDays => _consentOptions.TokenExpirationDays;

        [FromQuery(Name = "userId")]
        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; }

        [FromQuery(Name = "token")]
        [BindProperty(SupportsGet = true)]
        public string? Token { get; set; }

        /// <summary>
        /// GET only renders a confirmation page. It must NOT perform the irreversible
        /// consent/account-activation write, because email security scanners and
        /// link-preview clients routinely follow links automatically and would
        /// otherwise grant consent without the guardian intentionally acting.
        /// The actual mutation only happens in OnPostAsync, which is protected by the
        /// antiforgery token embedded in the confirmation form.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            if (UserId <= 0 || string.IsNullOrWhiteSpace(Token))
            {
                Message = "Nieprawidłowy link potwierdzający.";
                CanConfirm = false;
                return Page();
            }

            var (valid, message, childUserName, childEmail) = await _consentService.ValidateConsentTokenAsync(UserId, Token);
            CanConfirm = valid;
            ChildUserName = childUserName;
            ChildEmail = childEmail;
            Message = valid
                ? "Sprawdź poniższe informacje i potwierdź zgodę, klikając przycisk."
                : message;

            if (!valid)
            {
                _logger.LogWarning("Guardian consent link validation failed for user ID {UserId}: {Reason}", UserId, message);
            }

            return Page();
        }

        /// <summary>
        /// Performs the actual, irreversible consent confirmation and account activation.
        /// Only reachable via a POST from the confirmation form (antiforgery-protected).
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (UserId <= 0 || string.IsNullOrWhiteSpace(Token))
            {
                return RedirectToResult(false, "Nieprawidłowy link potwierdzający.");
            }

            // Get IP address from HttpContext
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Confirm consent atomically
            var (success, message) = await _consentService.ConfirmConsentAsync(UserId, Token, ipAddress);

            if (success)
            {
                _logger.LogInformation("Guardian consent confirmed for user ID {UserId}.", UserId);
                Message = message;
            }
            else
            {
                _logger.LogWarning("Failed to confirm guardian consent for user ID {UserId}: {Reason}", UserId, message);
                Message = message;
            }

            return RedirectToResult(success, message);
        }

        private IActionResult RedirectToResult(bool success, string message)
        {
            TempData["GuardianConsentSuccess"] = success;
            TempData["GuardianConsentMessage"] = message;
            return RedirectToPage("ConfirmGuardianConsentResult");
        }
    }
}
