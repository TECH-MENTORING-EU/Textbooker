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
        public bool IsSuccess { get; set; }
        public int TokenExpirationDays => _consentOptions.TokenExpirationDays;

        [FromQuery(Name = "userId")]
        public int UserId { get; set; }

        [FromQuery(Name = "token")]
        public string? Token { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (UserId <= 0 || string.IsNullOrWhiteSpace(Token))
            {
                Message = "Invalid confirmation link.";
                IsSuccess = false;
                return Page();
            }

            // Get IP address from HttpContext
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Confirm consent atomically
            var (success, message) = await _consentService.ConfirmConsentAsync(UserId, Token, ipAddress);

            if (success)
            {
                _logger.LogInformation("Guardian consent confirmed for user ID {UserId}.", UserId);
                Message = "Thank you! Your consent has been confirmed and the account is now active.";
            }
            else
            {
                _logger.LogWarning("Failed to confirm guardian consent for user ID {UserId}: {Reason}", UserId, message);
                Message = message;
            }

            IsSuccess = success;
            return Page();
        }
    }
}
