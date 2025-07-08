using System;
using Booker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;


namespace Booker.Services;

public class StaticDataManager
{
    private readonly DataContext _context;
    private readonly IMemoryCache _cache;

    public record Parameters(Grade? Grade, Subject? Subject, bool? Level);

    public StaticDataManager(DataContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<Grade>> GetGradesAsync()
    {
        if (!_cache.TryGetValue("grades", out List<Grade>? grades))
        {
            grades = await _context.Grades
                .OrderBy(g => g.Id)
                .ToListAsync();
            _cache.Set("grades", grades, TimeSpan.FromHours(1));
        }
        return grades!;
    }

    public async Task<List<Subject>> GetSubjectsAsync()
    {
        if (!_cache.TryGetValue("subjects", out List<Subject>? subjects))
        {
            subjects = await _context.Subjects
                .OrderBy(s => s.Name)
                .ToListAsync();
            _cache.Set("subjects", subjects, TimeSpan.FromHours(1));
        }
        return subjects!;
    }

    public async Task<Parameters> ConvertParametersAsync(string? grade, string? subject, string? level)
    {
        var _grades = await GetGradesAsync();
        var _subjects = await GetSubjectsAsync();

        return new Parameters
        (
            _grades.FirstOrDefault(g => g.GradeNumber == grade),
            _subjects.FirstOrDefault(s => s.Name == subject),
            level?.Equals("Rozszerzenie", StringComparison.OrdinalIgnoreCase) ?? false
        );
    }
}
