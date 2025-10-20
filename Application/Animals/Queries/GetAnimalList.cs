using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Animals.Queries;

/// <summary>
/// Defines the query and handler responsible for retrieving the list of animals from the database.
/// </summary>
/// <remarks>
/// Implements the MediatR <see cref="IRequest{TResponse}"/> and <see cref="IRequestHandler{TRequest, TResponse}"/>  
/// patterns to separate the query logic from the controller layer.  
/// The handler fetches all animals, including their associated breeds and images.
/// </remarks>
public class GetAnimalList
{
    /// <summary>
    /// Represents a request to retrieve a list of all animals from the database.
    /// </summary>
    /// <remarks>
    /// Used with MediatR to trigger the <see cref="Handler"/> that performs the data retrieval operation.
    /// </remarks>
    //public class Query : IRequest<List<Animal>>;

    /// <summary>
    /// Handles the <see cref="Query"/> request by retrieving all animals from the database.
    /// </summary>
    /// <remarks>
    /// Uses Entity Framework Core to asynchronously fetch the list of animals,
    /// including their related <see cref="Breed"/> and <see cref="Image"/> entities.  
    /// Returns the complete list to the caller through the MediatR pipeline.
    /// </remarks>
    //public class Handler(AppDbContext context)
    //    : IRequestHandler<Query, List<Animal>>
    //{

        /// <summary>
        /// Processes the <see cref="Query"/> request and retrieves all animals from the database.
        /// </summary>
        /// <param name="request">The query request (contains no parameters for this operation).</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation if needed.</param>
        /// <returns>
        /// A list of <see cref="Animal"/> entities, including their associated breed and image data.
        /// </returns>
        /// <remarks>
        /// Executes an asynchronous database query using Entity Framework Core,
        /// including related navigation properties (<c>Breed</c> and <c>Images</c>),
        /// and returns the result as a complete list.
        /// </remarks>
        //public async Task<List<Animal>> Handle(
        //    Query request,
        //    CancellationToken cancellationToken)
        //{
        //    var animals = await context.Animals
        //        .Include(x => x.Breed)
        //        .Include(x => x.Images)
        //        .ToListAsync(cancellationToken);

        //    return animals;
        //}
    //}
}