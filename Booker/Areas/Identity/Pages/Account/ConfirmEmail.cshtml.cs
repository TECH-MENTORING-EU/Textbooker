// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Booker.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly GuardianConsentService _consentService;
        private readonly ILogger<ConfirmEmailModel> _logger;

        public ConfirmEmailModel(
            UserManager<User> userManager,
            GuardianConsentService consentService,
            ILogger<ConfirmEmailModel> logger)
        {
            _userManager = userManager;
            _consentService = consentService;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public string DisplayMessage { get; set; }
        public bool CanManageProfile { get; private set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Nie znaleziono użytkownika o ID '{userId}'.");
            }

            if (user.EmailConfirmed)
            {
                StatusMessage = "Email jest już potwierdzony. Możesz się zalogować.";
                CanManageProfile = user.IsVisible;
                return Page();
            }

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                _logger.LogWarning("Email confirmation received an invalid token format for userId {UserId}.", userId);
                StatusMessage = "Błąd aktywacji konta. Link jest nieprawidłowy albo wygasł.";
                return Page();
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                _logger.LogWarning(
                    "Email confirmation failed for userId {UserId}. Errors: {Errors}",
                    userId,
                    errors);

                StatusMessage = "Błąd aktywacji konta. Link mógł wygasnąć albo został już użyty.";
                return Page();
            }

            CanManageProfile = user.IsVisible;

            // RODO - Phase 3: For minors, activation requires BOTH the child's own email
            // confirmation (verified here) AND the guardian's consent confirmation.
            // Marking the child's email as confirmed does not, by itself, use the guardian's
            // address for anything - it independently verifies the child owns their address.
            var consent = await _consentService.GetConsentAsync(user.Id);
            if (consent == null)
            {
                StatusMessage = "Twoje konto zostało pomyślnie aktywowane😉.";
                return Page();
            }

            if (consent.ConfirmedAtUtc.HasValue)
            {
                user.IsVisible = true;
                await _userManager.UpdateAsync(user);
                CanManageProfile = true;
                StatusMessage = "Twoje konto zostało pomyślnie aktywowane😉.";
            }
            else
            {
                StatusMessage = "Twój adres e-mail został potwierdzony. Czekamy jeszcze na zgodę opiekuna, aby aktywować konto.";
            }

            return Page();
        }
    }
}
