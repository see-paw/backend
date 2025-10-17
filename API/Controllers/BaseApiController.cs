using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace API.Controllers
{
    /// <summary>
    /// Serves as the base class for all API controllers.
    /// Provides shared access to the application's database context (<see cref="AppDbContext"/>)
    /// and optionally an <see cref="IMapper"/> instance for object mapping.
    /// </summary>
    /// <remarks>
    /// Any controller that inherits from <see cref="BaseApiController"/> can access
    /// <see cref="_dbContext"/> and, if applicable, <see cref="_mapper"/> directly.
    /// This promotes code reuse and consistent dependency injection across the API layer.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        /// <summary>
        /// The Entity Framework Core database context for accessing persistent data.
        /// </summary>
        protected readonly AppDbContext _dbContext;

        /// <summary>
        /// The AutoMapper instance used for object-to-object mapping.
        /// May be <see langword="null"/> if the derived controller does not require mapping.
        /// </summary>
        protected readonly IMapper? _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseApiController"/> class
        /// with both the database context and AutoMapper.
        /// </summary>
        /// <param name="context">The database context used to interact with persistence storage.</param>
        /// <param name="mapper">The AutoMapper instance used to convert between DTOs and entities.</param>
        /// <example>
        /// Controllers that use AutoMapper should inherit using:
        /// <code>
        /// public AnimalsController(AppDbContext context, IMapper mapper)
        ///     : base(context, mapper) { }
        /// </code>
        /// </example>
        public BaseApiController(AppDbContext context, IMapper mapper)
        {
            _dbContext = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseApiController"/> class
        /// with only the database context.
        /// </summary>
        /// <param name="context">The database context used to interact with persistence storage.</param>
        /// <remarks>
        /// This constructor should be used for controllers that do not depend on AutoMapper,
        /// such as read-only endpoints or simple query controllers.
        /// </remarks>
        /// <example>
        /// Controllers that do not use AutoMapper can inherit using:
        /// <code>
        /// public SheltersController(AppDbContext context)
        ///     : base(context) { }
        /// </code>
        /// </example>
        public BaseApiController(AppDbContext context)
        {
            _dbContext = context;
        }
    }
}
