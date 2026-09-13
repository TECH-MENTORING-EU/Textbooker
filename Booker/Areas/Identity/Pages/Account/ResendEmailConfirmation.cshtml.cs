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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Booker.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [EnableRateLimiting("IpRateLimit")]
    public class ResendEmailConfirmationModel : PageModel
    {
        // Anti-enumeration: always show the exact same message to the user regardless
        // of whether the account exists, is a minor account awaiting guardian consent,
        // or is a regular unconfirmed account. This prevents distinguishing valid
        // accounts (and their consent status) from unknown addresses.
        private const string GenericResendMessage =
            "Jeśli podany adres e-mail jest powiązany z kontem oczekującym na potwierdzenie, wysłaliśmy odpowiednią wiadomość z instrukcjami.";

        private readonly UserManager<User> _userManager;
        private readonly SendMailSvc _mailSvc;
        private readonly GuardianConsentService _consentService;
        private readonly DataContext _context;
        private readonly GuardianConsentOptions _consentOptions;
        private readonly ILogger<ResendEmailConfirmationModel> _logger;

        public ResendEmailConfirmationModel(
            UserManager<User> userManager,
            SendMailSvc mailSvc,
            GuardianConsentService consentService,
            DataContext context,
            IOptions<GuardianConsentOptions> consentOptions,
            ILogger<ResendEmailConfirmationModel> logger)
        {
            _userManager = userManager;
            _mailSvc = mailSvc;
            _consentService = consentService;
            _context = context;
            _consentOptions = consentOptions.Value;
            _logger = logger;
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
                DisplayMessage = GenericResendMessage;
                return Page();
            }

            // RODO - Phase 3: Minor accounts awaiting guardian consent get a new
            // guardian link (without revealing this distinct status to the caller).
            // Activation requires BOTH confirmations, so also resend the child's own
            // Identity confirmation email whenever it is still outstanding - otherwise
            // a child whose own confirmation message was lost/undelivered would have
            // no way to request a replacement while consent is pending.
            var pendingConsent = await _consentService.GetPendingConsentAsync(user.Id);
            if (pendingConsent != null)
            {
                await TryResendGuardianConsentAsync(user, pendingConsent);
                if (!user.EmailConfirmed)
                {
                    await SendEmailConfirmationAsync(user, isMinor: true);
                }

                DisplayMessage = GenericResendMessage;
                return Page();
            }

            await SendEmailConfirmationAsync(user, isMinor: false);

            DisplayMessage = GenericResendMessage;
            return Page();
        }

        private async Task SendEmailConfirmationAsync(User user, bool isMinor)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);

            // Mirror the initial-confirmation copy in Register.cshtml.cs: for a minor
            // account, the resent link only confirms the student's own email and does
            // not by itself activate the account, since guardian consent may still be
            // pending. Using the adult-only "your account is now active" copy here
            // would misstate the activation flow for this path.
            var body = isMinor
                ? $"Cześć! <br /> Cieszymy się, że dołączyłeś/dołączyłaś do społeczności TextBooker! <br /> Potwierdź swój adres e-mail, klikając w ten <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>link</a>. Twoje konto będzie także wymagało zgody opiekuna, zanim zostanie aktywowane. <br /><br /> Pozdrawiamy, <br /> Zespół TextBooker📚"
                : $"Cześć! <br /> Cieszymy się, że dołączyłeś/dołączyłaś do społeczności TextBooker! <br /> Twoje konto zostało pomyślnie utworzone. <br /> Kliknij w ten <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>link</a> aby aktywować konto. <br /><br /> Pozdrawiamy, <br /> Zespół TextBooker📚";

            await _mailSvc.SendEmailAsync(
                user.Email,
                "Witamy w TextBooker! Twoje konto zostało pomyślnie utworzone 🎉",
                body);
        }

        /// <summary>
        /// Resends the guardian consent email to the guardian.
        /// - Does NOT extend the account's original cleanup deadline (ExpiresAtUtc):
        ///   anyone who knows the student's email must not be able to keep an unconfirmed
        ///   account alive indefinitely by repeatedly calling this endpoint.
        /// - Rejects (silently, from the caller's point of view) resend attempts once the
        ///   original deadline has already passed; the account is due for cleanup.
        /// - Does not rotate the existing token on anonymous resend, so previously
        ///   delivered links remain valid until the original deadline.
        /// </summary>
        private async Task TryResendGuardianConsentAsync(User user, GuardianConsent pendingConsent)
        {
            if (DateTime.UtcNow > pendingConsent.ExpiresAtUtc)
            {
                _logger.LogInformation(
                    "Guardian consent resend skipped for user {UserId}: original deadline {ExpiresAtUtc} has passed.",
                    user.Id,
                    pendingConsent.ExpiresAtUtc);
                return;
            }

            var (newToken, _) = _consentService.GenerateToken();

            var confirmGuardianUrl = Url.Page(
                "/Account/ConfirmGuardianConsent",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, token = newToken },
                protocol: Request.Scheme);

            var sent = await _mailSvc.TrySendEmailAsync(
                pendingConsent.GuardianEmail,
                "TextBooker: Nowy link do potwierdzenia zgody",
                $"Oto nowy link do potwierdzenia zgody na konto ucznia: <a href='{HtmlEncoder.Default.Encode(confirmGuardianUrl)}'>Potwierdź zgodę</a>. Link ważny do {pendingConsent.ExpiresAtUtc:yyyy-MM-dd HH:mm} UTC.");
            if (!sent)
            {
                // Email delivery failed (already logged by SendMailSvc): keep the
                // previously issued token valid so the guardian's original link (if ever
                // delivered) still works.
                return;
            }

            // Deliberately do NOT touch RequestedAtUtc/ExpiresAtUtc - the original cleanup
            // deadline (tied to account creation) must not be extended by resends.
        }
    }
}
