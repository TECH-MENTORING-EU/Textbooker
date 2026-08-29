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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Booker.Services;
using System.Net;

namespace Booker.Areas.Identity.Pages.Account
{
    [EnableRateLimiting("IpRateLimit")]
    public class RegisterModel : PageModel
    {
        // TODO scalanie: użyć stałej z zadania 02 (Booker.Utilities.RegulaminInfo.CurrentVersion) zamiast poniższej
        private const string AcceptedRegulaminVersion = "1.0";

        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly DataContext _context;
        private readonly SchoolMappingService _schoolMappingService;
        private readonly IWebHostEnvironment _environment;

        public RegisterModel(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            DataContext context,
            SchoolMappingService schoolMappingService,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _schoolMappingService = schoolMappingService;
            _environment = environment;
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
            [Range(typeof(bool), "true", "true", ErrorMessage = "Musisz zaakceptować regulamin.")]
            [Display(Name = "Przeczytałem/am i akceptuję regulamin.")]
            public bool AcceptTerms { get; set; }

            [Required(ErrorMessage = "Musisz potwierdzić wiek lub zgodę opiekuna.")]
            [Range(typeof(bool), "true", "true", ErrorMessage = "Musisz potwierdzić wiek lub zgodę opiekuna.")]
            [Display(Name = "Oświadczam, że ukończyłem/am 16 lat, albo posiadam zgodę rodzica lub opiekuna prawnego na korzystanie z Serwisu.")]
            public bool ConfirmsAgeRequirement { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var activeSchools = await _context.Schools.Where(s => s.IsActive).OrderBy(s => s.Id).ToListAsync();
            AvailableSchools = new SelectList(activeSchools, "Id", "Name");
            Input ??= new InputModel();
        }

        public async Task<IActionResult> OnGetAutoSchoolAsync([FromQuery(Name = "Input.Email")] string email)
        {
            var activeSchools = await _context.Schools
                .Where(s => s.IsActive)
                .OrderBy(s => s.Id)
                .ToListAsync();

            var options = new StringBuilder();
            options.Append("<option value=\"\">Wybierz szkołę</option>");

            if (activeSchools.Count == 0)
            {
                return Content(options.ToString(), "text/html; charset=utf-8");
            }

            var schoolId = string.IsNullOrWhiteSpace(email)
                ? null
                : await _schoolMappingService.GetSchoolIdByEmailAsync(email);

            if (schoolId.HasValue)
            {
                var matchedSchool = activeSchools.FirstOrDefault(s => s.Id == schoolId.Value);
                if (matchedSchool is not null)
                {
                    var matchedOption = $"<option value=\"{matchedSchool.Id}\" selected>{WebUtility.HtmlEncode(matchedSchool.Name)}</option>";
                    return Content(matchedOption, "text/html; charset=utf-8");
                }
            }

            foreach (var school in activeSchools)
            {
                options.Append($"<option value=\"{school.Id}\">{WebUtility.HtmlEncode(school.Name)}</option>");
            }

            return Content(options.ToString(), "text/html; charset=utf-8");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var activeSchools = await _context.Schools.Where(s => s.IsActive).OrderBy(s => s.Id).ToListAsync();
            AvailableSchools = new SelectList(activeSchools, "Id", "Name");
            Input ??= new InputModel();
            
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                var autoAssignedSchoolId = await _schoolMappingService.GetSchoolIdByEmailAsync(Input.Email);
                if (autoAssignedSchoolId.HasValue)
                {
                    user.SchoolId = autoAssignedSchoolId.Value;
                    _logger.LogInformation(
                        "User automatically assigned to school ID {SchoolId} based on email domain",
                        autoAssignedSchoolId.Value
                    );
                }
                else if (Input.SchoolId.HasValue)
                {
                    var selectedSchool = await _context.Schools
                        .FirstOrDefaultAsync(s => s.Id == Input.SchoolId.Value && s.IsActive);

                    if (selectedSchool is null)
                    {
                        _logger.LogWarning(
                            "User tried to register with inactive/nonexistent school ID {SchoolId}",
                            Input.SchoolId.Value
                        );
                        ModelState.AddModelError(
                            nameof(Input.SchoolId),
                            "Wybrana szkoła nie jest dostępna. Wybierz inną szkołę."
                        );
                        return Page();
                    }

                    user.SchoolId = Input.SchoolId.Value;
                    _logger.LogInformation(
                        "User manually assigned to school ID {SchoolId}",
                        Input.SchoolId.Value
                    );
                }
                else
                {
                    ModelState.AddModelError(
                        nameof(Input.SchoolId),
                        "Nie znaleziono szkoły dla podanego adresu e-mail. Wybierz szkołę ręcznie."
                    );
                    return Page();
                }

                user.Photo = "/img/default-profile-picture.jpg";

                var now = DateTime.Now;
                user.TermsAcceptedAt = now;
                // TODO scalanie: użyć stałej z zadania 02 (Booker.Utilities.RegulaminInfo.CurrentVersion) zamiast poniższej
                user.TermsAcceptedVersion = AcceptedRegulaminVersion;
                user.AgeConfirmationAcceptedAt = now;

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