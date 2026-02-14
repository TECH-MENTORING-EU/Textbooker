using Booker.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Booker.Services;

/// <summary>
/// Service responsible for managing schools including CRUD operations, 
/// soft delete, and database schema management.
/// </summary>
public class SchoolService
{
    private readonly DataContext _context;
    private readonly ILogger<SchoolService> _logger;

    public SchoolService(DataContext context, ILogger<SchoolService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all schools (optionally including inactive ones).
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive schools</param>
    /// <returns>List of schools</returns>
    public async Task<List<School>> GetAllSchoolsAsync(bool includeInactive = false)
    {
        var query = _context.Schools.AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(s => s.IsActive);
        }
        
        return await query.OrderBy(s => s.Name).ToListAsync();
    }

    /// <summary>
    /// Gets all schools with user count.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive schools</param>
    /// <returns>List of schools with user counts</returns>
    public async Task<List<SchoolWithUserCount>> GetSchoolsWithUserCountAsync(bool includeInactive = false)
    {
        var query = _context.Schools.AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(s => s.IsActive);
        }
        
        return await query
            .Select(s => new SchoolWithUserCount
            {
                School = s,
                UserCount = s.Users.Count
            })
            .OrderBy(s => s.School.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a school by ID.
    /// </summary>
    /// <param name="id">School ID</param>
    /// <returns>School or null if not found</returns>
    public async Task<School?> GetSchoolByIdAsync(int id)
    {
        return await _context.Schools.FindAsync(id);
    }

    /// <summary>
    /// Gets active schools only (for normal user dropdowns).
    /// </summary>
    /// <returns>List of active schools</returns>
    public async Task<List<School>> GetActiveSchoolsAsync()
    {
        return await _context.Schools
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new school and its database schema.
    /// </summary>
    /// <param name="name">School name</param>
    /// <param name="emailDomain">Email domain for auto-assignment (optional)</param>
    /// <returns>The created school</returns>
    public async Task<School> CreateSchoolAsync(string name, string? emailDomain = null)
    {
        

        var school = new School
        {
            Name = name,
            EmailDomain = emailDomain,
            CreatedAt = DateTime.UtcNow
        };

        _context.Schools.Add(school);
        await _context.SaveChangesAsync();


        _logger.LogInformation(
            "Created new school: {SchoolName} (ID: {SchoolId}",
            school.Name,
            school.Id
        );

        return school;
    }

    /// <summary>
    /// Soft deletes a school by setting IsActive to false.
    /// The schema is preserved for data retention.
    /// </summary>
    /// <param name="id">School ID to soft delete</param>
    /// <returns>True if successful, false if school not found</returns>
    public async Task<bool> SoftDeleteSchoolAsync(int id)
    {
        var school = await _context.Schools.FindAsync(id);
        if (school == null)
        {
            _logger.LogWarning("Attempted to soft delete non-existent school with ID: {SchoolId}", id);
            return false;
        }

        school.DeactivatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Soft deleted school: {SchoolName} (ID: {SchoolId}). Schema {SchemaName} preserved.",
            school.Name,
            school.Id
        );

        return true;
    }

    /// <summary>
    /// Reactivates a soft-deleted school.
    /// </summary>
    /// <param name="id">School ID to reactivate</param>
    /// <returns>True if successful, false if school not found</returns>
    public async Task<bool> ReactivateSchoolAsync(int id)
    {
        var school = await _context.Schools.FindAsync(id);
        if (school == null)
        {
            _logger.LogWarning("Attempted to reactivate non-existent school with ID: {SchoolId}", id);
            return false;
        }
        
        school.DeactivatedAt = null;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Reactivated school: {SchoolName} (ID: {SchoolId})",
            school.Name,
            school.Id
        );

        return true;
    }

    /// <summary>
    /// Updates school information.
    /// </summary>
    /// <param name="id">School ID</param>
    /// <param name="name">New name</param>
    /// <param name="emailDomain">New email domain</param>
    /// <returns>Updated school or null if not found</returns>
    public async Task<School?> UpdateSchoolAsync(int id, string name, string? emailDomain)
    {
        var school = await _context.Schools.FindAsync(id);
        if (school == null)
        {
            return null;
        }

        // Validate email domain uniqueness (excluding this school)
        if (!string.IsNullOrWhiteSpace(emailDomain))
        {
            var normalizedDomain = emailDomain.Trim().ToLower();
            var domainInUse = await _context.Schools
                .Where(s => s.Id != id && s.IsActive && s.EmailDomain != null)
                .AnyAsync(s => s.EmailDomain!.ToLower().Contains(normalizedDomain));

            if (domainInUse)
            {
                _logger.LogWarning(
                    "Cannot update school {SchoolId}: email domain '{Domain}' is already in use by another active school",
                    id, normalizedDomain
                );
                throw new InvalidOperationException($"Email domain '{normalizedDomain}' is already in use by another active school.");
            }
        }

        school.Name = name;
        school.EmailDomain = emailDomain;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Updated school: {SchoolName} (ID: {SchoolId})",
            school.Name,
            school.Id
        );

        return school;
    }

    /// <summary>
    /// Generates a valid schema name from school name.
    /// </summary>
    /// <param name="schoolName">School name</param>
    /// <returns>Valid schema name</returns>
    private static string GenerateSchemaName(string schoolName)
    {
        // Remove diacritics and special characters, convert to lowercase
        var normalized = RemoveDiacritics(schoolName).ToLowerInvariant();
        
        // Replace spaces and special chars with underscores
        var schemaName = Regex.Replace(normalized, @"[^a-z0-9]", "_");
        
        // Remove consecutive underscores
        schemaName = Regex.Replace(schemaName, @"_+", "_");
        
        // Trim underscores from start and end
        schemaName = schemaName.Trim('_');
        
        // Ensure it starts with a letter
        if (schemaName.Length > 0 && char.IsDigit(schemaName[0]))
        {
            schemaName = "school_" + schemaName;
        }
        
        // Limit length
        if (schemaName.Length > 50)
        {
            schemaName = schemaName[..50];
        }
        
        return string.IsNullOrEmpty(schemaName) ? "school" : schemaName;
    }

    /// <summary>
    /// Removes diacritics from a string.
    /// </summary>
    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        var builder = new System.Text.StringBuilder();
        
        foreach (var c in normalized)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }
        
        return builder.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }

    /// <summary>
    /// Validates that a schema name is safe to use.
    /// </summary>
    private static bool IsValidSchemaName(string schemaName)
    {
        if (string.IsNullOrWhiteSpace(schemaName))
            return false;
            
        // Only allow alphanumeric and underscore
        return Regex.IsMatch(schemaName, @"^[a-z][a-z0-9_]*$");
    }

    /// <summary>
    /// Gets the count of users in a school.
    /// </summary>
    /// <param name="schoolId">School ID</param>
    /// <returns>Number of users</returns>
    public async Task<int> GetUserCountAsync(int schoolId)
    {
        return await _context.Users.CountAsync(u => u.SchoolId == schoolId);
    }
}

/// <summary>
/// DTO for school with user count.
/// </summary>
public class SchoolWithUserCount
{
    public required School School { get; set; }
    public int UserCount { get; set; }
}
