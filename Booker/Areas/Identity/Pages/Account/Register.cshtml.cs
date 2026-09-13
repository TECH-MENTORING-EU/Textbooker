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
using Booker.Utilities;
using System.Net;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly SchoolMappingService _schoolMappingService;
        private readonly IWebHostEnvironment _environment;
        private readonly GuardianConsentService _consentService;

        public RegisterModel(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            DataContext context,
            SchoolMappingService schoolMappingService,
            IWebHostEnvironment environment,
            GuardianConsentService consentService)
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
            _consentService = consentService;
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
            [StringLength(100, ErrorMessage = "{0} musi mieć co najmniej {2}, a maksymalnie {1} znaków.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Hasło")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Potwierdź hasło")]
            [Compare("Password", ErrorMessage = "Hasła się nie zgadzają.")]
            public string ConfirmPassword { get; set; }
            
            [Required(ErrorMessage = "Musisz wybrać szkołę.")]
            [Display(Name = "Szkoła")]
            public int? SchoolId { get; set; }

            [Display(Name = "Mam co najmniej 16 lat")]
            public bool IsAtLeast16 { get; set; }

            [EmailAddress(ErrorMessage = "E-mail opiekuna musi być prawidłowym adresem e-mail.")]
            [Display(Name = "E-mail opiekuna")]
            public string GuardianEmail { get; set; }

            [MustBeTrue(ErrorMessage = "Musisz zaakceptować regulamin.")]
            [Display(Name = "Przeczytałem/am i akceptuję regulamin.")]
            public bool AcceptTerms { get; set; }
        }

        public record GuardianEmailFieldModel(bool IsVisible, string GuardianEmail);

        // Derived from the posted (or bound) checkbox so the guardian email field
        // stays visible - and its validation error visible - after any server-side
        // failure on a full-page re-render (e.g. failed POST), not just via the
        // htmx partial swap.
        public bool ShowGuardianEmailField => Input is { IsAtLeast16: false };

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

        public IActionResult OnGetGuardianField(
            [FromQuery(Name = "Input.IsAtLeast16")] bool isAtLeast16 = false,
            [FromQuery(Name = "Input.GuardianEmail")] string guardianEmail = null)
        {
            return Partial("_GuardianEmailField", new GuardianEmailFieldModel(!isAtLeast16, guardianEmail));
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var activeSchools = await _context.Schools.Where(s => s.IsActive).OrderBy(s => s.Id).ToListAsync();
            AvailableSchools = new SelectList(activeSchools, "Id", "Name");
            Input ??= new InputModel();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!TryValidateRegistration())
            {
                return Page();
            }

            var isAtLeast16 = Input.IsAtLeast16;
            var user = CreateUser();
            if (!await TryAssignSchoolAsync(user))
            {
                return Page();
            }

            InitializeUser(user, isAtLeast16);
            var registration = await CreateAccountAsync(user, isAtLeast16);

            if (!registration.Result.Succeeded)
            {
                AddIdentityErrors(registration.Result);
                return Page();
            }

            // SendEmailAsync never throws (delivery failures are logged inside it), so a
            // transient failure here can't surface as a 500 - the account is already
            // created and committed above, and the user can request a replacement link
            // via ResendEmailConfirmation.
            await SendConfirmationAsync(user, isAtLeast16, registration, returnUrl);

            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, isMinor = !isAtLeast16, returnUrl });
        }

        private bool TryValidateRegistration()
        {
            var guardianEmailError = _consentService.ValidateGuardianEmail(Input.Email, Input.GuardianEmail, Input.IsAtLeast16);
            if (guardianEmailError is null)
            {
                return true;
            }

            ModelState.AddModelError("Input.GuardianEmail", guardianEmailError);
            return false;
        }

        private async Task<bool> TryAssignSchoolAsync(User user)
        {
            var autoAssignedSchoolId = await _schoolMappingService.GetSchoolIdByEmailAsync(Input.Email);
            if (autoAssignedSchoolId.HasValue)
            {
                user.SchoolId = autoAssignedSchoolId.Value;
                _logger.LogInformation(
                    "User automatically assigned to school ID {SchoolId} based on email domain",
                    autoAssignedSchoolId.Value);
                return true;
            }

            if (!Input.SchoolId.HasValue)
            {
                ModelState.AddModelError(
                    "Input.SchoolId",
                    "Nie znaleziono szkoły dla podanego adresu e-mail. Wybierz szkołę ręcznie.");
                return false;
            }

            var selectedSchool = await _context.Schools
                .FirstOrDefaultAsync(s => s.Id == Input.SchoolId.Value && s.IsActive);
            if (selectedSchool is null)
            {
                _logger.LogWarning(
                    "User tried to register with inactive/nonexistent school ID {SchoolId}",
                    Input.SchoolId.Value);
                ModelState.AddModelError(
                    "Input.SchoolId",
                    "Wybrana szkoła nie jest dostępna. Wybierz inną szkołę.");
                return false;
            }

            user.SchoolId = selectedSchool.Id;
            _logger.LogInformation(
                "User manually assigned to school ID {SchoolId}",
                selectedSchool.Id);
            return true;
        }

        private void InitializeUser(User user, bool isAtLeast16)
        {
            user.Photo = "/img/default-profile-picture.jpg";
            user.TermsAcceptedAt = DateTime.Now;
            user.TermsAcceptedVersion = RegulaminInfo.CurrentVersion;

            if (isAtLeast16)
            {
                user.AgeConfirmationAcceptedAt = DateTime.UtcNow;
                return;
            }

            user.EmailConfirmed = false;
            user.IsVisible = false;
        }

        private async Task<RegistrationResult> CreateAccountAsync(User user, bool isAtLeast16)
        {
            await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            if (isAtLeast16)
            {
                return new RegistrationResult(
                    await _userManager.CreateAsync(user, Input.Password),
                    null,
                    default);
            }

            return await CreateMinorAccountAsync(user);
        }

        private async Task<RegistrationResult> CreateMinorAccountAsync(User user)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new RegistrationResult(result, null, default);
                }

                var consentResult = await _consentService.CreateConsentAsync(user, Input.GuardianEmail!);
                _context.GuardianConsents.Add(consentResult.Consent);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new RegistrationResult(
                    result,
                    consentResult.Token,
                    consentResult.Consent.ExpiresAtUtc);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task SendConfirmationAsync(
            User user,
            bool isAtLeast16,
            RegistrationResult registration,
            string returnUrl)
        {
            if (!isAtLeast16)
            {
                // RODO - Phase 3: Minors must independently confirm they own their own
                // email address, in addition to the guardian confirming consent.
                // Activation requires both steps to complete.
                await SendGuardianConfirmationAsync(user, registration);
                await SendEmailConfirmationAsync(user, returnUrl, isMinor: true);
                return;
            }

            await SendEmailConfirmationAsync(user, returnUrl, isMinor: false);
        }

        private async Task SendGuardianConfirmationAsync(User user, RegistrationResult registration)
        {
            var confirmGuardianUrl = Url.Page(
                "/Account/ConfirmGuardianConsent",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, token = registration.GuardianToken },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                Input.GuardianEmail,
                            "TextBooker: Prośba o wyrażenie zgody na konto ucznia",
                BuildGuardianConsentEmailBody(
                    user.UserName,
                    Input.Email,
                    confirmGuardianUrl,
                    registration.GuardianConsentExpiresAt));

            _logger.LogInformation(
                "Minor user {UserName} (ID: {UserId}) registered; guardian consent sent to {GuardianEmail}.",
                user.UserName,
                user.Id,
                Input.GuardianEmail);
        }

        private async Task SendEmailConfirmationAsync(User user, string returnUrl, bool isMinor)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId, code, returnUrl },
                protocol: Request.Scheme);

            var body = isMinor
                ? $"Potwierdź swój adres e-mail, klikając <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>tutaj</a>. " +
                  "Twoje konto będzie także wymagało zgody opiekuna, zanim zostanie aktywowane."
                : $"Potwierdź swoje konto, klikając <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>tutaj</a>.";

            await _emailSender.SendEmailAsync(
                Input.Email,
                "Potwierdź swój adres e-mail",
                body);

            _logger.LogInformation(
                "{AccountType} user {UserName} (ID: {UserId}) registered.",
                isMinor ? "Minor" : "Adult",
                user.UserName,
                user.Id);
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private record RegistrationResult(
            IdentityResult Result,
            string GuardianToken,
            DateTime GuardianConsentExpiresAt);

        private string BuildGuardianConsentEmailBody(string studentUsername, string studentEmail, string confirmUrl, DateTime expiresAtUtc)
        {
            return $@"
                <html>
                <body>
                <p>Witaj,</p>
                <p>Uczeń zarejestrowany jako <strong>{HtmlEncoder.Default.Encode(studentUsername ?? "unknown")}</strong>
                z adresem e-mail <strong>{HtmlEncoder.Default.Encode(studentEmail)}</strong>
                chce korzystać z platformy TextBooker, która wymaga Twojej zgody jako opiekuna.</p>
                
                <p>Konto będzie aktywne dopiero po Twojej zgodzie i potwierdzeniu własnego adresu e-mail przez ucznia. Jeśli nie rozpoznajesz tej prośby, zignoruj tę wiadomość.</p>
                
                <p><a href='{HtmlEncoder.Default.Encode(confirmUrl)}'>Potwierdź zgodę klikając tutaj</a></p>
                
                <p>Link ważny do {expiresAtUtc:yyyy-MM-dd HH:mm} UTC.</p>
                <p>Pozdrawiamy,<br/>Zespół TextBooker</p>
                </body>
                </html>";
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