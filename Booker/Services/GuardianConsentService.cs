using Booker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Booker.Services;

/// <summary>
/// RODO - Phase 1: Service for managing guardian consent for minors (<16 years).
/// Handles guardian-email validation, token generation (hashed), and atomic confirmation workflow.
/// </summary>
public class GuardianConsentService
{
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;
    private readonly GuardianConsentOptions _options;
    private readonly ILogger<GuardianConsentService> _logger;

    private const int TokenLength = 32; // 32 bytes = 256 bits for SHA-256

    public GuardianConsentService(
        DataContext context,
        UserManager<User> userManager,
        IOptions<GuardianConsentOptions> options,
        ILogger<GuardianConsentService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates guardian email for users who did not self-declare as being at least 16.
    /// - When not confirmed as 16+: guardianEmail must be non-null, valid, and different from childEmail (case-insensitive).
    /// - When confirmed as 16+: guardianEmail is ignored and validation passes.
    /// Returns error message if invalid, null if valid.
    /// </summary>
    public string? ValidateGuardianEmail(string? childEmail, string? guardianEmail, bool isAtLeast16)
    {
        // Self-declared adults don't need guardian email
        if (isAtLeast16)
            return null;

        // Minors must have guardian email
        if (string.IsNullOrWhiteSpace(guardianEmail))
            return "E-mail opiekuna jest wymagany dla ucznia niepełnoletniego.";

        if (string.IsNullOrWhiteSpace(childEmail))
            return "E-mail ucznia jest wymagany.";

        // Normalize emails to match Identity's email comparer (case-insensitive)
        string normalizedChild = _userManager.NormalizeEmail(childEmail);
        string normalizedGuardian = _userManager.NormalizeEmail(guardianEmail);

        // Guardian and child emails must be different (after normalization)
        if (normalizedChild == normalizedGuardian)
            return "E-mail opiekuna musi być inny niż adres e-mail ucznia.";

        return null;
    }

    /// <summary>
    /// Creates a guardian consent record with a cryptographically random token.
    /// Returns the GuardianConsent with the plain token (for URL construction).
    /// The token is NOT saved to the database; only the SHA-256 hash is saved.
    /// </summary>
    public async Task<(GuardianConsent Consent, string Token)> CreateConsentAsync(User childUser, string guardianEmail)
    {
        if (childUser == null)
            throw new ArgumentNullException(nameof(childUser));

        if (string.IsNullOrWhiteSpace(guardianEmail))
            throw new ArgumentException("Guardian email cannot be null or empty.", nameof(guardianEmail));

        var (plainToken, tokenHash) = GenerateToken();

        var now = DateTime.UtcNow;
        var consent = new GuardianConsent
        {
            UserId = childUser.Id,
            GuardianEmail = guardianEmail.Trim(),
            TokenHash = tokenHash,
            RequestedAtUtc = now,
            ExpiresAtUtc = now.AddDays(_options.TokenExpirationDays),
            ConfirmedAtUtc = null,
            ConfirmationIpAddress = null
        };

        // Do NOT save to database here; let the caller handle the save
        _logger.LogInformation("Created guardian consent record for user {UserId}. Token expires at {ExpiresAtUtc}.",
            childUser.Id, consent.ExpiresAtUtc);

        return (consent, plainToken);
    }

    /// <summary>
    /// Confirms guardian consent if the token is valid, not expired, and not already confirmed.
    /// Atomically updates User.EmailConfirmed = true, User.IsVisible = true, and saves audit data.
    /// Returns (Success=true, message) if confirmed, (Success=false, message) if failed.
    /// </summary>
    public async Task<(bool Success, string Message)> ConfirmConsentAsync(int userId, string token, string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, "Link potwierdzający jest nieprawidłowy.");

        string tokenHash = ComputeTokenHash(token);
        var now = DateTime.UtcNow;

        try
        {
            // Use a transaction to ensure atomic updates
            await using (var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable))
            {
                // Get the user and pending consent
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Confirmation attempted for non-existent user {UserId}.", userId);
                    return (false, "Link potwierdzający jest nieprawidłowy.");
                }

                var consent = await _context.GuardianConsents.FirstOrDefaultAsync(gc => gc.UserId == userId);
                if (consent == null)
                {
                    _logger.LogWarning("Confirmation attempted for user {UserId} without pending consent.", userId);
                    return (false, "Link potwierdzający jest nieprawidłowy.");
                }

                // Authenticate the token before revealing consent state.
                if (!CryptographicOperations.FixedTimeEquals(
                        Convert.FromBase64String(consent.TokenHash),
                        Convert.FromBase64String(tokenHash)))
                {
                    _logger.LogWarning("Confirmation attempted with invalid token hash for user {UserId}.", userId);
                    return (false, "Link potwierdzający jest nieprawidłowy.");
                }

                if (consent.ConfirmedAtUtc.HasValue)
                {
                    _logger.LogWarning("Confirmation attempted for already-confirmed consent {ConsentId}.", consent.Id);
                    return (false, "Ten link potwierdzający został już wykorzystany.");
                }

                if (now > consent.ExpiresAtUtc)
                {
                    _logger.LogWarning("Confirmation attempted with expired token for user {UserId}. Expired at {ExpiresAtUtc}.",
                        userId, consent.ExpiresAtUtc);
                    return (false, "Ten link potwierdzający wygasł. Poproś o nowy link.");
                }

                // Token is valid (verified above using a constant-time comparison);
                // record guardian consent. Do NOT mark the child's own email as
                // confirmed here - the guardian's address is not the child's, so
                // email ownership must still be verified independently by the child
                // via the normal ConfirmEmail flow. Only activate (IsVisible = true)
                // once both guardian consent AND the child's own email confirmation
                // are done.
                consent.ConfirmedAtUtc = now;
                consent.ConfirmationIpAddress = ipAddress;

                string message;
                if (user.EmailConfirmed)
                {
                    user.IsVisible = true;
                    _context.Users.Update(user);
                    message = "Zgoda opiekuna została potwierdzona. Twoje konto jest teraz aktywne.";
                }
                else
                {
                    message = "Zgoda opiekuna została potwierdzona. Konto zostanie aktywowane, gdy uczeń potwierdzi swój adres e-mail.";
                }

                _context.GuardianConsents.Update(consent);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Guardian consent confirmed for user {UserId}. Child email confirmed: {EmailConfirmed}. IP: {IpAddress}",
                    userId, user.EmailConfirmed, ipAddress ?? "unknown");

                return (true, message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming consent for user {UserId}.", userId);
            return (false, "Wystąpił błąd podczas przetwarzania potwierdzenia. Spróbuj ponownie.");
        }
    }

    /// <summary>
    /// Generates a cryptographically random token and its SHA-256 hash.
    /// The plain token is never persisted; only the hash is stored.
    /// </summary>
    public (string Token, string TokenHash) GenerateToken()
    {
        byte[] tokenBytes = new byte[TokenLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }

        string plainToken = Convert.ToBase64String(tokenBytes);
        string tokenHash = ComputeTokenHash(plainToken);
        return (plainToken, tokenHash);
    }

    /// <summary>
    /// Read-only validation of a guardian consent token: checks the user/consent exist,
    /// the token has not been used, is not expired, and the hash matches.
    /// Does NOT mutate any state. Intended to be safe to call from a GET request
    /// (e.g. to render a confirmation page) so that automated link-crawlers/scanners
    /// cannot trigger the irreversible consent side effects.
    /// </summary>
    public async Task<(bool Valid, string Message, string? ChildUserName, string? ChildEmail)> ValidateConsentTokenAsync(int userId, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, "Link potwierdzający jest nieprawidłowy.", null, null);

        string tokenHash = ComputeTokenHash(token);
        var now = DateTime.UtcNow;

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "Link potwierdzający jest nieprawidłowy.", null, null);

        var consent = await _context.GuardianConsents.FirstOrDefaultAsync(gc => gc.UserId == userId);
        if (consent == null)
            return (false, "Link potwierdzający jest nieprawidłowy.", null, null);

        if (consent.ConfirmedAtUtc.HasValue)
            return (false, "Ten link potwierdzający został już wykorzystany.", null, null);

        if (now > consent.ExpiresAtUtc)
            return (false, "Ten link potwierdzający wygasł. Poproś o nowy link.", null, null);

        if (consent.TokenHash != tokenHash)
            return (false, "Link potwierdzający jest nieprawidłowy.", null, null);

        return (true, "Link potwierdzający jest prawidłowy.", user.UserName, user.Email);
    }

    /// <summary>
    /// Retrieves pending (unconfirmed) guardian consent for a user.
    /// Returns null if no pending consent exists.
    /// </summary>
    public async Task<GuardianConsent?> GetPendingConsentAsync(int userId)
    {
        return await _context.GuardianConsents
            .FirstOrDefaultAsync(gc => gc.UserId == userId && gc.ConfirmedAtUtc == null);
    }

    /// <summary>
    /// Retrieves the guardian consent record for a user regardless of confirmation status.
    /// Returns null if the user never required guardian consent.
    /// </summary>
    public async Task<GuardianConsent?> GetConsentAsync(int userId)
    {
        return await _context.GuardianConsents
            .FirstOrDefaultAsync(gc => gc.UserId == userId);
    }

    /// <summary>
    /// Computes SHA-256 hash of the token for storage in database.
    /// </summary>
    private string ComputeTokenHash(string token)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
            byte[] hashBytes = sha256.ComputeHash(tokenBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
