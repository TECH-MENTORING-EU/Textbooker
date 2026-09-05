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
        private readonly DataContext _context;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            DataContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // RODO - task 06: the school can't be changed from this form - deliberately not part
        // of [BindProperty] Input, so no extra field in the POST body can overwrite it.
        public string SchoolName { get; set; }

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

            [Display(Name = "Pokaż mój numer telefonu jako dostępną formę kontaktu")]
            public bool DisplayPhone { get; set; }

            [Display(Name = "Pokaż WhatsApp jako dostępną formę kontaktu")]
            public bool DisplayWhatsapp { get; set; }

            [Display(Name = "Messenger (nazwa użytkownika)")]
            public string FbMessenger { get; set; }

            [Display(Name = "Pokaż Messenger jako dostępną formę kontaktu")]
            public bool DisplayMessenger { get; set; }

            [Display(Name = "Instagram (nazwa użytkownika)")]
            public string Instagram { get; set; }

            [Display(Name = "Pokaż Instagram jako dostępną formę kontaktu")]
            public bool DisplayInstagram { get; set; }

            [Display(Name = "Pokaż moje ulubione innym użytkownikom")]
            public bool AreFavoritesPublic { get; set; }

            [Display(Name = "Pokaż moją szkołę przy moich ogłoszeniach")]
            public bool DisplaySchool { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);


            Username = userName;

            SchoolName = user.SchoolId.HasValue
                ? (await _context.Schools.FindAsync(user.SchoolId.Value))?.Name
                : null;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                AreFavoritesPublic = user.AreFavoritesPublic,
                DisplayEmail = user.DisplayEmail,
                DisplayPhone = user.DisplayPhone,
                DisplayWhatsapp = user.DisplayWhatsapp,
                FbMessenger = user.FbMessenger,
                DisplayMessenger = user.DisplayMessenger,
                Instagram = user.Instagram,
                DisplayInstagram = user.DisplayInstagram,
                DisplaySchool = user.DisplaySchool
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

            // The phone number/Messenger/Instagram only count as a contact method when their display switch is on.
            // Validate against the trimmed value, matching what actually gets persisted below - a
            // whitespace-only entry must not be treated as a configured contact channel.
            var trimmedFbMessenger = Input.FbMessenger?.Trim();
            var trimmedInstagram = Input.Instagram?.Trim();
            var phoneDisplayed = !string.IsNullOrEmpty(Input.PhoneNumber) && (Input.DisplayPhone || Input.DisplayWhatsapp);
            var messengerDisplayed = Input.DisplayMessenger && !string.IsNullOrWhiteSpace(trimmedFbMessenger);
            var instagramDisplayed = Input.DisplayInstagram && !string.IsNullOrWhiteSpace(trimmedInstagram);

            if (!Input.DisplayEmail
                && !phoneDisplayed
                && !messengerDisplayed
                && !instagramDisplayed)
            {
                ModelState.AddModelError(string.Empty, "Musisz wybrać przynajmniej jedną formę kontaktu.");
            }

            if (Input.DisplayWhatsapp && string.IsNullOrEmpty(Input.PhoneNumber))
            {
                ModelState.AddModelError("Input.PhoneNumber", "Aby wybrać WhatsApp jako formę kontaktu, musisz podać numer telefonu.");
            }

            if (Input.DisplayMessenger && string.IsNullOrWhiteSpace(trimmedFbMessenger))
            {
                ModelState.AddModelError("Input.FbMessenger", "Aby wybrać Messenger jako formę kontaktu, musisz podać nazwę użytkownika.");
            }

            if (Input.DisplayInstagram && string.IsNullOrWhiteSpace(trimmedInstagram))
            {
                ModelState.AddModelError("Input.Instagram", "Aby wybrać Instagram jako formę kontaktu, musisz podać nazwę użytkownika.");
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
            user.DisplayPhone = Input.DisplayPhone;
            user.DisplayWhatsapp = Input.DisplayWhatsapp;
            user.FbMessenger = trimmedFbMessenger;
            user.DisplayMessenger = Input.DisplayMessenger;
            user.Instagram = trimmedInstagram;
            user.DisplayInstagram = Input.DisplayInstagram;
            user.DisplaySchool = Input.DisplaySchool;

            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Twój profil został zaktualizowany.";
            return RedirectToPage();
        }
    }
}
