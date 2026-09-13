// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Booker.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly GuardianConsentOptions _consentOptions;

        public RegisterConfirmationModel(IOptions<GuardianConsentOptions> consentOptions)
        {
            _consentOptions = consentOptions.Value;
        }

        [BindProperty(SupportsGet = true)]
        public string? Email { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool IsMinor { get; set; }

        public int TokenExpirationDays => _consentOptions.TokenExpirationDays;

        public IActionResult OnGet()
        {
            return string.IsNullOrEmpty(Email) ? RedirectToPage("/Index") : Page();
        }
    }
}
