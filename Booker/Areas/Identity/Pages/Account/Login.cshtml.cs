// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Booker.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Booker.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        // Single message for every failed sign-in attempt. Distinct messages for
        // unknown email, wrong password, unconfirmed, or locked-out accounts let
        // an attacker enumerate registered addresses (issue #66).
        // RODO - task 07: extended with a mention of password reset, without revealing
        // whether the cause is a wrong name/password or a lockout after too many attempts -
        // the same sentence covers both cases.
        private const string GenericLoginFailureMessage =
            "Błędny login lub hasło. Jeśli logowanie wielokrotnie się nie udaje, odczekaj chwilę " +
            "albo zresetuj hasło.";

        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public LoginModel(SignInManager<User> signInManager, ILogger<LoginModel> logger, UserManager<User> userManager, IWebHostEnvironment environment)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _environment = environment;
        }

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
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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
            [Display(Name = "Nazwa użytkownika / e-mail")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Hasło")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Zapamiętaj mnie")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var userName = Input.Email;
                if (userName.IndexOf('@') > -1)
                {
                    // TODO: validate email format (regex?)
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, GenericLoginFailureMessage);
                        return Page();
                    }
                    userName = user.UserName;
                }
                var result = await _signInManager.PasswordSignInAsync(userName, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(userName);
                    if (user != null)
                    {
                        user.LastActiveAt = DateTime.Now;
                        await _userManager.UpdateAsync(user);
                    }
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Konto użytkownika {UserName} zostało zablokowane po zbyt dużej liczbie nieudanych prób logowania.", userName);
                    // The Lockout page discloses the lockout end time, so it must
                    // stay unreachable from the public login flow outside development.
                    if (_environment.IsDevelopment())
                    {
                        var user = await _userManager.FindByNameAsync(userName);
                        if (user?.LockoutEnd is DateTimeOffset lockoutEnd)
                        {
                            return RedirectToPage("./Lockout", new { lockoutEnd = lockoutEnd.ToUnixTimeSeconds() });
                        }
                    }
                }
                // Wrong password, unconfirmed account, and (outside development)
                // lockout all end here on purpose: the response must not reveal
                // whether the account exists or what state it is in.
                ModelState.AddModelError(string.Empty, GenericLoginFailureMessage);
                return Page();
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
