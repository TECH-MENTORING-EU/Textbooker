// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Booker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
            /// 
            [Display(Name = "Pokaż mój e-mail jako dostępną formę kontaktu")]
            public bool DisplayEmail { get; set; }

            [Phone]
            [Display(Name = "Numer telefonu")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Pokaż WhatsApp jako dostępną formę kontaktu")]
            public bool DisplayWhatsapp { get; set; }

            [Display(Name = "Messenger (nazwa użytkownika)")]
            public string FbMessenger { get; set; }

            [Display(Name = "Instagram (nazwa użytkownika)")]
            public string Instagram { get; set; }

            [Display(Name = "Pokaż moje ulubione innym użytkownikom")]
            public bool AreFavoritesPublic { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);


            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                AreFavoritesPublic = user.AreFavoritesPublic,
                DisplayEmail = user.DisplayEmail,
                DisplayWhatsapp = user.DisplayWhatsapp,
                FbMessenger = user.FbMessenger,
                Instagram = user.Instagram
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Nie znaleziono użytkownika o ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Nie znaleziono użytkownika o ID '{_userManager.GetUserId(User)}'.");
            }

            if (!Input.DisplayEmail 
                && string.IsNullOrEmpty(Input.PhoneNumber) 
                && string.IsNullOrEmpty(Input.FbMessenger) 
                && string.IsNullOrEmpty(Input.Instagram))
            {
                ModelState.AddModelError(string.Empty, "Musisz wybrać przynajmniej jedną formę kontaktu.");
            }

            if (Input.DisplayWhatsapp && string.IsNullOrEmpty(Input.PhoneNumber))
            {
                ModelState.AddModelError("Input.PhoneNumber", "Aby wybrać WhatsApp jako formę kontaktu, musisz podać numer telefonu.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Wystąpił nieznany błąd podczas próby zmiany numeru telefonu.";
                    return RedirectToPage();
                }
            }

            
            user.AreFavoritesPublic = Input.AreFavoritesPublic;
            user.DisplayEmail = Input.DisplayEmail;
            user.DisplayWhatsapp = Input.DisplayWhatsapp;
            user.FbMessenger = Input.FbMessenger?.Trim();
            user.Instagram = Input.Instagram?.Trim();

            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Twój profil został zaktualizowany.";
            return RedirectToPage();
        }
    }
}
