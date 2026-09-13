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
/// Handles age validation, token generation (hashed), and atomic confirmation workflow.
/// </summary>
public class GuardianConsentService
{
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;
    private readonly GuardianConsentOptions _options;
    private readonly ILogger<GuardianConsentService> _logger;

    private const int MinBirthYear = 1910;
    private const int AgeThreshold = 16;
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
    /// Calculates age based on birth year and current UTC year.
    /// Returns -1 for invalid years (future or extremely old).
    /// </summary>
    public int CalculateAge(int birthYear)
    {
        int currentYear = DateTime.UtcNow.Year;
        return birthYear > currentYear ? -1 : currentYear - birthYear;
    }

    /// <summary>
    /// Validates birth year: not null, not in future, within reasonable range.
    /// Returns true if valid, false otherwise.
    /// </summary>
    public bool IsValidBirthYear(int? birthYear)
    {
        if (!birthYear.HasValue)
            return false;

        int year = birthYear.Value;
        int currentYear = DateTime.UtcNow.Year;

        // Year must not be in the future or extremely old
        if (year > currentYear || year < MinBirthYear)
            return false;

        return true;
    }

    /// <summary>
    /// Validates guardian email for minors (<16 years).
    /// - For age < 16: guardianEmail must be non-null, valid, and different from childEmail (case-insensitive).
    /// - For age >= 16: guardianEmail is ignored and validation passes.
    /// Returns error message if invalid, null if valid.
    /// </summary>
    public string? ValidateGuardianEmail(string? childEmail, string? guardianEmail, int age)
    {
        // Adults (>=16) don't need guardian email
        if (age >= AgeThreshold)
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

        // Generate cryptographically random 32-byte token
        byte[] tokenBytes = new byte[TokenLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }

        string plainToken = Convert.ToBase64String(tokenBytes);
        string tokenHash = ComputeTokenHash(plainToken);

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

                // Check if already confirmed
                if (consent.ConfirmedAtUtc.HasValue)
                {
                    _logger.LogWarning("Confirmation attempted for already-confirmed consent {ConsentId}.", consent.Id);
                    return (false, "Ten link potwierdzający został już wykorzystany.");
                }

                // Check if token is expired
                if (now > consent.ExpiresAtUtc)
                {
                    _logger.LogWarning("Confirmation attempted with expired token for user {UserId}. Expired at {ExpiresAtUtc}.",
                        userId, consent.ExpiresAtUtc);
                    return (false, "Ten link potwierdzający wygasł. Poproś o nowy link.");
                }

                // Verify token hash
                if (consent.TokenHash != tokenHash)
                {
                    _logger.LogWarning("Confirmation attempted with invalid token hash for user {UserId}.", userId);
                    return (false, "Link potwierdzający jest nieprawidłowy.");
                }

                // Token is valid; confirm consent and activate user
                consent.ConfirmedAtUtc = now;
                consent.ConfirmationIpAddress = ipAddress;

                user.EmailConfirmed = true;
                user.IsVisible = true;

                _context.GuardianConsents.Update(consent);
                _context.Users.Update(user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Guardian consent confirmed for user {UserId}. Account activated. IP: {IpAddress}",
                    userId, ipAddress ?? "unknown");

                return (true, "Zgoda opiekuna została potwierdzona. Twoje konto jest teraz aktywne.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming consent for user {UserId}.", userId);
            return (false, "Wystąpił błąd podczas przetwarzania potwierdzenia. Spróbuj ponownie.");
        }
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
