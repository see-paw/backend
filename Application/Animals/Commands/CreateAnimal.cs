using Application.Core;
using Application.Images.Commands;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Animals.Commands
{
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

        public class Handler(AppDbContext dbContext, IImageAppService<Animal> imageAppService) : IRequestHandler<Command, Result<string>>
        {
            public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                if (request.Files.Count != request.Images.Count)
                    return Result<string>.Failure("Mismatch between files and image metadata.", 400);

                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    var shelterTask = dbContext.Shelters
                        .AnyAsync(s => s.Id == request.ShelterId, cancellationToken);
                    
                    if (!shelterTask.Result)
                        return Result<string>.Failure("Shelter not found", 404);

                    var breedTask = string.IsNullOrEmpty(request.Animal.BreedId)
                        ? Task.FromResult(true)
                        : dbContext.Breeds.AnyAsync(b => b.Id == request.Animal.BreedId, cancellationToken);
                    
                    if (!breedTask.Result)
                        return Result<string>.Failure("Breed not found", 404);

                    //Assign the shelter ID to the animal
                    request.Animal.ShelterId = request.ShelterId;

                    // Persist the entity in the database
                    dbContext.Animals.Add(request.Animal);
                    var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

                    if (!success)
                        return Result<string>.Failure("Failed to create animal", 400);

                    for (int i = 0; i < request.Files.Count; i++)
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
                            //If image upload failed, rollback animal insertion without images
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
