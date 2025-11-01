using Microsoft.EntityFrameworkCore;

namespace Application.Core
{
    /// <summary>
    /// Provides a generic pagination helper that wraps a list of items along with pagination metadata.
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the paginated list.</typeparam>
    public class PagedList<T> : List<T>
    {
        /// <summary>
        /// The current page number being displayed.
        /// </summary>
        public int CurrentPage { get; private set; } // private set ensures only the class can modify this value

        /// <summary>
        /// The total number of available pages based on the total count and page size.
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// The number of items included in each page.
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// The total number of items across all pages.
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class with the specified items and pagination metadata.
        /// </summary>
        /// <param name="items">The list of items to include in the current page.</param>
        /// <param name="count">The total number of items in the data source.</param>
        /// <param name="pageNumber">The current page number (starting from 1).</param>
        /// <param name="pageSize">The number of items per page.</param>
        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
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
        /// <param name="source">The <see cref="IQueryable{T}"/> data source, typically an EF Core query.</param>
        /// <param name="pageNumber">The page number to retrieve (starting from 1).</param>
        /// <param name="pageSize">The number of items to include per page.</param>
        /// <returns>
        /// A task representing the asynchronous operation, which returns a <see cref="PagedList{T}"/> instance
        /// containing the items for the requested page and the corresponding pagination metadata.
        /// </returns>
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            // Count the total number of records in the source
            var count = await source.CountAsync();

            // Retrieve the subset of items for the requested page
            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return a new paginated list instance
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
