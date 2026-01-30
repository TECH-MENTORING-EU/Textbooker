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
        // Generate schema name from school name
        var schemaName = GenerateSchemaName(name);
        
        // Check if schema name already exists
        var existingSchool = await _context.Schools
            .FirstOrDefaultAsync(s => s.SchemaName == schemaName);
        
        if (existingSchool != null)
        {
            // Add unique suffix if schema name exists
            var suffix = 1;
            while (await _context.Schools.AnyAsync(s => s.SchemaName == $"{schemaName}_{suffix}"))
            {
                suffix++;
            }
            schemaName = $"{schemaName}_{suffix}";
        }

        var school = new School
        {
            Name = name,
            EmailDomain = emailDomain,
            SchemaName = schemaName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Schools.Add(school);
        await _context.SaveChangesAsync();

        // Create the database schema
        await CreateSchemaAsync(schemaName);

        _logger.LogInformation(
            "Created new school: {SchoolName} (ID: {SchoolId}, Schema: {SchemaName})",
            school.Name,
            school.Id,
            school.SchemaName
        );

        return school;
    }

    /// <summary>
    /// Creates a database schema for a school.
    /// </summary>
    /// <param name="schemaName">Name of the schema to create</param>
    private async Task CreateSchemaAsync(string schemaName)
    {
        try
        {
            // Validate schema name to prevent SQL injection
            if (!IsValidSchemaName(schemaName))
            {
                throw new ArgumentException($"Invalid schema name: {schemaName}");
            }

            // Create the schema using raw SQL
            // Schema name is validated to be alphanumeric with underscores only
            var sql = $"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'{schemaName}') EXEC('CREATE SCHEMA [{schemaName}]')";
            #pragma warning disable EF1002 // Schema name is validated by IsValidSchemaName to prevent SQL injection
            await _context.Database.ExecuteSqlRawAsync(sql);
            #pragma warning restore EF1002

            _logger.LogInformation("Created database schema: {SchemaName}", schemaName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database schema: {SchemaName}", schemaName);
            throw;
        }
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

        school.IsActive = false;
        school.DeactivatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Soft deleted school: {SchoolName} (ID: {SchoolId}). Schema {SchemaName} preserved.",
            school.Name,
            school.Id,
            school.SchemaName
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

        school.IsActive = true;
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
            schemaName = schemaName.Substring(0, 50);
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
