using Booker.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Booker.Pages;

public class PrivacyModel : PageModel
{
    private readonly ILogger<PrivacyModel> _logger;
    private readonly GuardianConsentOptions _consentOptions;

    public PrivacyModel(ILogger<PrivacyModel> logger, IOptions<GuardianConsentOptions> consentOptions)
    {
        _logger = logger;
        _consentOptions = consentOptions.Value;
    }

    // RODO - Phase 3: The policy text quotes these values instead of hardcoding them,
    // so an appsettings change to the guardian consent configuration cannot silently
    // make the published legal text wrong.
    public int GuardianTokenExpirationDays => _consentOptions.TokenExpirationDays;
    public int ConfirmedDataRetentionDays => _consentOptions.ConfirmedDataRetentionDays;

    public void OnGet()
    {
    }
}