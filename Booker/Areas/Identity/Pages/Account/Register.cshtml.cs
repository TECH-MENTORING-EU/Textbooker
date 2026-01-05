#nullable disable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Booker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Booker.Areas.Identity.Pages.Account
{
    [EnableRateLimiting("IpRateLimit")]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly DataContext _context;

        public RegisterModel(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            DataContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        
        public SelectList AvailableSchools { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Nazwa użytkownika")]
            public string UserName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "E-mail")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "{0} musi mieć co najmniej {2}, a maksymalnie {1} znaków.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Hasło")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Potwierdź hasło")]
            [Compare("Password", ErrorMessage = "Hasła się nie zgadzają.")]
            public string ConfirmPassword { get; set; }
            
            [Display(Name = "Szkoła")]
            public int? SchoolId { get; set; }

            [Required(ErrorMessage = "Musisz zaakceptować regulamin.")]
            [Display(Name = "Przeczytałem/am i akceptuję regulamin.")]
            public bool AcceptTerms { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            AvailableSchools = new SelectList(await _context.Schools.ToListAsync(), "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            AvailableSchools = new SelectList(await _context.Schools.ToListAsync(), "Id", "Name");
            
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                user.SchoolId = Input.SchoolId; // Use selected school from form
                user.Photo = "/img/default-profile-picture.jpg";

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Użytkownik {user.UserName} utworzył nowe konto.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Witamy w TextBooker! Twoje konto zostało pomyślnie utworzone 🎉",
                        $"Cześć! <br /> Cieszymy się, że dołączyłeś/dołączyłaś do społeczności TextBooker! <br /> Twoje konto zostało pomyślnie utworzone. <br /> Kliknij w ten <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>link</a> aby aktywować konto. <br /><br /> Pozdrawiamy, <br /> Zespół TextBooker📚");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Nie można utworzyć użytkownika o nazwie '{nameof(User)}'. " +
                    $"Upewnij się, że '{nameof(User)}' nie jest abstrakcyjną klasą i ma bezparametrowy konstruktor, alternatywnie " +
                    $"nadpisz stronę rejestracji w /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("Podstawowy interfejs użytkownika wymaga dowstawcy usługi mailowej.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}