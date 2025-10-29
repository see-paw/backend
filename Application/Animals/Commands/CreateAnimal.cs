using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Animals.Commands
{
    /// <summary>
    /// Provides the command logic to create a new <see cref="Animal"/> entity,
    /// including database persistence and associated image uploads.
    /// </summary>
    /// <remarks>
    /// This class follows the CQRS (Command Query Responsibility Segregation) pattern,
    /// implemented through the <see cref="MediatR"/> library. It encapsulates both the
    /// request data (via <see cref="Command"/>) and the corresponding handler logic 
    /// (via <see cref="Handler"/>).
    ///
    /// Responsibilities:
    /// • Validar consistência entre ficheiros e metadados de imagem.
    /// • Verificar existência do CAA e da raça do animal.
    /// • Persistir a entidade <see cref="Animal"/> na base de dados.
    /// • Invocar o serviço de imagem (<see cref="IImageAppService{T}"/>) para upload.
    /// • Gerir transações e rollback em caso de falha.
    /// </remarks>
    public class CreateAnimal
    {
        /// <summary>
        /// Represents the command request to create a new animal.
        /// Implements <see cref="IRequest{TResponse}"/> returning a <see cref="Result{T}"/> 
        /// with the created animal's unique identifier.
        /// </summary>
        public class Command : IRequest<Result<string>>
        {
            /// <summary>
            /// The animal entity containing all its biological and adoption attributes.
            /// </summary>
            public required Animal Animal { get; init; }

            /// <summary>
            /// The unique identifier of the shelter where the animal is hosted.
            /// </summary>
            public required string ShelterId { get; init; }

            /// <summary>
            /// The metadata of the images associated with the animal.
            /// </summary>
            public required List<Image> Images { get; init; }

            /// <summary>
            /// The actual uploaded image files.
            /// </summary>
            public required List<IFormFile> Files { get; init; }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Handler"/> class.
        /// </summary>
        /// <param name="dbContext">The application's database context.</param>
        /// <param name="imageAppService">The service used to handle image uploads for animals.</param>
        public class Handler(AppDbContext dbContext, IImageAppService<Animal> imageAppService) : IRequestHandler<Command, Result<string>>
        {

            /// <summary>
            /// Executes the logic to create a new <see cref="Animal"/> and its related images.
            /// </summary>
            /// <param name="request">The <see cref="Command"/> containing the animal data, shelter ID, and image files.</param>
            /// <param name="cancellationToken">A cancellation token to monitor for operation cancellation.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> containing the unique identifier of the created animal
            /// if successful, or an error message with a corresponding HTTP status code otherwise.
            /// </returns>
            /// <exception cref="Exception">
            /// Thrown when an unexpected error occurs during database or image processing.
            /// </exception>
            /// <remarks>
            /// Validation rules:
            /// - The number of image files must match the number of image metadata entries.
            /// - At least one image must be provided.
            /// - The specified shelter and breed must exist.
            ///
            /// Transaction management:
            /// - All operations are executed inside a database transaction.
            /// - Rollback is performed if any validation or upload step fails.
            ///
            /// Return codes:
            /// - 201: Success (animal created)
            /// - 400: Invalid request (validation or upload error)
            /// - 404: Related entity not found (shelter/breed)
            /// - 500: Unexpected internal error
            /// </remarks>
            public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                if (request.Files.Count != request.Images.Count)
                    return Result<string>.Failure("Mismatch between files and image metadata.", 400);

                if (request.Files.Count == 0)
                    return Result<string>.Failure("At least one image is required.", 400);

                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    var shelterExists = await dbContext.Shelters
                        .AnyAsync(s => s.Id == request.ShelterId, cancellationToken);
                    
                    if (!shelterExists)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<string>.Failure("Shelter not found", 404);
                    }

                    if (string.IsNullOrEmpty(request.Animal.BreedId))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<string>.Failure("Breed not found", 404);
                    }

                    var breedExists = await dbContext.Breeds
                        .AnyAsync(b => b.Id == request.Animal.BreedId, cancellationToken);
                    
                    if (!breedExists)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<string>.Failure("Breed not found", 404);
                    }

                    request.Animal.ShelterId = request.ShelterId;

                    dbContext.Animals.Add(request.Animal);
                    var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

                    if (!success)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<string>.Failure("Failed to create animal", 400);
                    }

                    for (var i = 0; i < request.Files.Count; i++)
                    {
                        var file = request.Files[i];
                        var meta = request.Images[i];

                        var imgResult = await imageAppService.AddImageAsync(
                            dbContext,
                            request.Animal.Id,
                            file,
                            meta.Description ?? string.Empty,
                            meta.IsPrincipal,
                            cancellationToken
                        );

                        if (!imgResult.IsSuccess)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Result<string>.Failure($"Image upload failed: {imgResult.Error}", 400);
                        }
                    }

                    await transaction.CommitAsync(cancellationToken);
                    
                    return Result<string>.Success(request.Animal.Id, 201);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<string>.Failure($"Unexpected error: {ex.Message}", 500);
                }
            }
        }

    }
}