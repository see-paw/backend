using Microsoft.EntityFrameworkCore;

namespace API.Core;

/// <summary>
/// Generic pagination helper that wraps a list of items with metadata.
/// </summary>
public class PagedList<T> : List<T>
{
    public int CurrentPage { get; private set; }//private set only the class can change the value
    public int TotalPages { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }

    private PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        AddRange(items);
    }

    /// <summary>
    /// Creates a paginated list asynchronously from a queryable data source.
    /// </summary>
    /// <param name="source">The IQueryable data source (e.g., EF Core query).</param>
    /// <param name="pageNumber">The current page number (starting from 1).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, 
    /// returning a <see cref="PagedList{T}"/> containing the paginated data and metadata.
    /// </returns>
    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
