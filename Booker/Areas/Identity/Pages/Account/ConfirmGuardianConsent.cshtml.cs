using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ConfirmGuardianConsentModel> _logger;
        private readonly GuardianConsentOptions _consentOptions;

        public ConfirmGuardianConsentModel(
            GuardianConsentService consentService,
            IEmailSender emailSender,
            ILogger<ConfirmGuardianConsentModel> logger,
            IOptions<GuardianConsentOptions> consentOptions)
        {
            _consentService = consentService;
            _emailSender = emailSender;
            _logger = logger;
            _consentOptions = consentOptions.Value;
        }

        public string? Message { get; set; }

        /// <summary>
        /// True when the GET-supplied link is valid and safe to present a confirmation
        /// form for. False for invalid/expired/used links.
        /// </summary>
        public bool CanConfirm { get; set; }

        public string? StudentUserName { get; set; }
        public string? StudentEmail { get; set; }

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

            var validation = await _consentService.ValidateConsentTokenAsync(UserId, Token);
            CanConfirm = validation.IsValid;
            StudentUserName = validation.StudentUserName;
            StudentEmail = validation.StudentEmail;
            Message = validation.IsValid
                ? "Sprawdź poniższe informacje i potwierdź zgodę, klikając przycisk."
                : validation.Message;

            if (!validation.IsValid)
            {
                _logger.LogWarning("Guardian consent link validation failed for user ID {UserId}: {Reason}", UserId, validation.Message);
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
            var consentResult = await _consentService.ConfirmConsentAsync(UserId, Token, ipAddress);

            if (consentResult.Success)
            {
                _logger.LogInformation("Guardian consent confirmed for user ID {UserId}.", UserId);
                Message = consentResult.Message;

                if (consentResult.AccountActivated && !string.IsNullOrEmpty(consentResult.StudentEmail))
                {
                    await _emailSender.SendEmailAsync(
                        consentResult.StudentEmail,
                        "Witamy w TextBooker! Twoje konto zostało pomyślnie utworzone 🎉",
                        "Cześć! <br /> Cieszymy się, że dołączyłeś/dołączyłaś do społeczności TextBooker! <br /> Twoje konto zostało pomyślnie aktywowane. Możesz się już zalogować. <br /><br /> Pozdrawiamy, <br /> Zespół TextBooker📚");
                }
            }
            else
            {
                _logger.LogWarning("Failed to confirm guardian consent for user ID {UserId}: {Reason}", UserId, consentResult.Message);
                Message = consentResult.Message;
            }

            return RedirectToResult(consentResult.Success, consentResult.Message);
        }

        private IActionResult RedirectToResult(bool success, string message)
        {
            // Keys must match the [TempData] property names on
            // ConfirmGuardianConsentResultModel (IsSuccess/Message), otherwise the
            // redirected result page silently falls back to its failure branch.
            TempData["IsSuccess"] = success;
            TempData["Message"] = message;
            return RedirectToPage("ConfirmGuardianConsentResult");
        }
    }
}
