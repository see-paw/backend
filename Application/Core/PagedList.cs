using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace Application.Core
{
    /// <summary>
    /// Represents a paginated collection of items with metadata such as total count and current page.
    /// </summary>
    public class PagedList<T> : IEnumerable<T>
    {
        /// <summary>
        /// The items contained in the current page.
        /// </summary>
        public List<T> Items { get; private set; } = new();

        /// <summary>
        /// The current page number.
        /// </summary>
        public int CurrentPage { get; private set; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        public int TotalCount { get; private set; }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items ?? new List<T>();
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }

        /// <summary>
        /// Creates a paginated list from a queryable source asynchronously.
        /// </summary>
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        /// <summary>
        /// Enables iteration directly on PagedList.
        /// </summary>
        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
