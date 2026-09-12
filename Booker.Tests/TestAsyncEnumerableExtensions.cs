namespace Booker.Tests;

public static class TestAsyncEnumerableExtensions
{
    /// <summary>
    /// Materializes an IAsyncEnumerable into a list. Deliberately NOT named ToListAsync:
    /// on the net10 test host the BCL's System.Linq AsyncEnumerable and the transitively
    /// referenced System.Linq.Async 6.0.3 declare identical extensions, which makes any
    /// ToListAsync call ambiguous (CS0121).
    /// </summary>
    public static async Task<List<T>> Materialize<T>(this IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
        {
            list.Add(item);
        }
        return list;
    }
}
