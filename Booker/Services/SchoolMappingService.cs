using Booker.Data;
using Microsoft.EntityFrameworkCore;

namespace Booker.Services;

/// <summary>
/// Service responsible for mapping email domains to schools
/// </summary>
public class SchoolMappingService
{
    private readonly DataContext _context;
    private readonly ILogger<SchoolMappingService> _logger;

    public SchoolMappingService(DataContext context, ILogger<SchoolMappingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Attempts to find a school based on the user's email domain.
    /// Returns null if no matching school is found or if multiple schools share the same domain.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>SchoolId if a unique match is found, null otherwise</returns>
    public async Task<int?> GetSchoolIdByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("GetSchoolIdByEmail called with null or empty email");
            return null;
        }

        var emailDomain = ExtractDomain(email);
        if (string.IsNullOrEmpty(emailDomain))
        {
            _logger.LogWarning("Could not extract domain from provided email");
            return null;
        }

        // Find all active schools that have this domain in their EmailDomain field
        var matchingSchools = await _context.Schools
            .Where(s => s.IsActive && s.EmailDomain != null && s.EmailDomain.Contains(emailDomain))
            .ToListAsync();

        // Filter schools where the domain matches exactly (handles comma-separated domains)
        var exactMatches = matchingSchools
            .Where(s => s.EmailDomain!
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim().ToLower())
                .Contains(emailDomain.ToLower()))
            .ToList();

        if (exactMatches.Count == 0)
        {
            _logger.LogInformation("No school found for email domain: {Domain}", emailDomain);
            return null;
        }

        if (exactMatches.Count > 1)
        {
            _logger.LogWarning(
                "Multiple schools ({Count}) found for email domain: {Domain}. Schools: {SchoolIds}",
                exactMatches.Count,
                emailDomain,
                string.Join(", ", exactMatches.Select(s => $"{s.Id}:{s.Name}"))
            );
            return null; // Prevent ambiguous assignment
        }

        var school = exactMatches[0];
        _logger.LogInformation(
            "Successfully mapped email domain {Domain} to school {SchoolName} (ID: {SchoolId})",
            emailDomain,
            school.Name,
            school.Id
        );

        return school.Id;
    }

    /// <summary>
    /// Extracts the domain from an email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Domain part of the email (e.g., "example.com")</returns>
    private static string? ExtractDomain(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex == -1 || atIndex == email.Length - 1)
        {
            return null;
        }

        return email.Substring(atIndex + 1).Trim();
    }

    /// <summary>
    /// Validates if a domain is already associated with an active school
    /// </summary>
    /// <param name="domain">Email domain to check</param>
    /// <returns>True if domain is already in use by an active school, false otherwise</returns>
    public async Task<bool> IsDomainInUseAsync(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return false;
        }

        var normalizedDomain = domain.Trim().ToLower();
        var schools = await _context.Schools
            .Where(s => s.IsActive && s.EmailDomain != null)
            .ToListAsync();

        return schools.Any(s => s.EmailDomain!
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(d => d.Trim().ToLower())
            .Contains(normalizedDomain));
    }
}
