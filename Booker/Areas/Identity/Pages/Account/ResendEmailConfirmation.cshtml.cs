// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Booker.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [EnableRateLimiting("IpRateLimit")]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly GuardianConsentService _consentService;
        private readonly DataContext _context;
        private readonly GuardianConsentOptions _consentOptions;

        public ResendEmailConfirmationModel(
            UserManager<User> userManager,
            IEmailSender emailSender,
            GuardianConsentService consentService,
            DataContext context,
            IOptions<GuardianConsentOptions> consentOptions)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _consentService = consentService;
            _context = context;
            _consentOptions = consentOptions.Value;
        }

        public string DisplayMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Wiadomość z linkiem aktywacyjnym konta została wysłana. Sprawdź swoją skrzynkę e-mail.");
                return Page();
            }

            // RODO - Phase 3: Redirect minor to wait for guardian consent
            var pendingConsent = await _consentService.GetPendingConsentAsync(user.Id);
            if (pendingConsent != null)
            {
                await ResendGuardianConsentAsync(user, pendingConsent);
                // This is a minor account awaiting guardian consent
                DisplayMessage = "To konto oczekuje na zgodę opiekuna. " +
                    "Link potwierdzający został wysłany na adres e-mail opiekuna. " +
                    "Poczekaj, aż opiekun potwierdzi zgodę.";
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                Input.Email,
                "Witamy w TextBooker! Twoje konto zostało pomyślnie utworzone 🎉",
                $"Cześć! <br /> Cieszymy się, że dołączyłeś/dołączyłaś do społeczności TextBooker! <br /> Twoje konto zostało pomyślnie utworzone. <br /> Kliknij w ten <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>link</a> aby aktywować konto. <br /><br /> Pozdrawiamy, <br /> Zespół TextBooker📚");

            ModelState.AddModelError(string.Empty, "Wiadomość z linkiem aktywacyjnym konta została wysłana. Sprawdź swoją skrzynkę e-mail.");
            return Page();
        }

        // Helper method for resending to guardian
        private async Task ResendGuardianConsentAsync(User user, GuardianConsent pendingConsent)
        {
            // Generate new token
            var (newConsent, newToken) = await _consentService.CreateConsentAsync(user, pendingConsent.GuardianEmail);

            // Update the existing consent record
            pendingConsent.TokenHash = newConsent.TokenHash;
            pendingConsent.RequestedAtUtc = newConsent.RequestedAtUtc;
            pendingConsent.ExpiresAtUtc = newConsent.ExpiresAtUtc;

            await _context.SaveChangesAsync();

            // Build and send new link to guardian
            var confirmGuardianUrl = Url.Page(
                "/Account/ConfirmGuardianConsent",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, token = newToken },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                pendingConsent.GuardianEmail,
                "TextBooker: Nowy link do potwierdzenia zgody",
                $"Oto nowy link do potwierdzenia zgody na konto ucznia: <a href='{HtmlEncoder.Default.Encode(confirmGuardianUrl)}'>Potwierdź zgodę</a>. Link ważny przez {_consentOptions.TokenExpirationDays} dni.");
        }
    }
}
