using System;
using Booker.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;


namespace Booker.Services;

public class StaticDataManager
{
    private readonly DataContext _context;
    private readonly IMemoryCache _cache;

    public record Parameters(string? Title, Grade? Grade, Subject? Subject, Level? Level);

    public StaticDataManager(DataContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Book?> GetBookAsync(int id) =>
        (await GetBooksAsync()).FirstOrDefault(b => b.Id == id);

    public async Task<List<Book>> GetBooksAsync()
    {
        if (!_cache.TryGetValue("books", out List<Book>? books))
        {
            books = await _context.Books
                .Include(b => b.Grades)
                .Include(b => b.Subject)
                .Include(b => b.Level)
                .OrderBy(g => g.Id)
                .ToListAsync();
            _cache.Set("books", books, TimeSpan.FromHours(1));
        }
        return books!;
    }

    public async Task<List<Book>> GetBooksByTitleAsync(string title)
    {
        var books = await GetBooksAsync();
        return ApplyTitleFilter(books, title).ToList();
    }

    public async Task<List<Book>> GetBooksByParamsAsync(Parameters input)
    {
        var books = await GetBooksAsync();
        return ApplyFilters(books, input).ToList();
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

    public async Task<List<Grade>> GetGradesByBookTitleAsync(string title)
    {
        var books = await GetBooksByTitleAsync(title);
        return books.SelectMany(b => b.Grades).Distinct().ToList();
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

    public async Task<List<Subject>> GetSubjectsByBookTitleAsync(string title)
    {
        var books = await GetBooksByTitleAsync(title);
        return books.Select(b => b.Subject).Distinct().ToList();
    }

    public async Task<List<Level>> GetLevelsAsync()
    {
        if (!_cache.TryGetValue("levels", out List<Level>? levels))
        {
            levels = await _context.Levels
                .OrderBy(l => l.Id)
                .ToListAsync();
            _cache.Set("levels", levels, TimeSpan.FromHours(1));
        }
        return levels!;
    }

    public async Task<List<Level>> GetLevelsByBookTitleAsync(string title)
    {
        var books = await GetBooksByTitleAsync(title);
        return books.Select(b => b.Level).Distinct().ToList();
    }

    public async Task<Parameters> ConvertParametersAsync(string? title, string? grade, string? subject, string? level)
    {
        var _grades = await GetGradesAsync();
        var _subjects = await GetSubjectsAsync();
        var _levels = await GetLevelsAsync();

        return new Parameters
        (
            title,
            _grades.FirstOrDefault(g => g.GradeNumber == grade),
            _subjects.FirstOrDefault(s => s.Name == subject),
            _levels.FirstOrDefault(l => l.Name == level)
        );
    }


    private static IEnumerable<Book> ApplyFilters(IEnumerable<Book> query, Parameters input)
    {
        query = ApplyTitleFilter(query, input.Title);
        query = ApplyGradeFilter(query, input.Grade);
        query = ApplySubjectFilter(query, input.Subject);
        query = ApplyLevelFilter(query, input.Level);

        return query;
    }

    private static IEnumerable<Book> ApplyTitleFilter(IEnumerable<Book> query, string? title)
    {
        return string.IsNullOrWhiteSpace(title)
            ? query
            : query.Where(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<Book> ApplyGradeFilter(IEnumerable<Book> query, Grade? grade)
    {
        return grade == null
            ? query
            : query.Where(b => b.Grades.Any(g => g.Id == grade.Id));
    }

    private static IEnumerable<Book> ApplySubjectFilter(IEnumerable<Book> query, Subject? subject)
    {
        return subject == null
            ? query
            : query.Where(b => b.Subject.Id == subject.Id);
    }

    private static IEnumerable<Book> ApplyLevelFilter(IEnumerable<Book> query, Level? level)
    {
        return level == null
            ? query
            : query.Where(b => b.Level == level);
    }
}
