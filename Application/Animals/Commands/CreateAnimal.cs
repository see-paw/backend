using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Common;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application.Animals.Commands
{
    /// <summary>
    /// Provides the command logic to create a new <see cref="Animal"/> entity,
    /// including database persistence and associated image uploads.
    /// </summary>
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
        /// <param name="uploadService">The service used to handle image uploads for animals.</param>
        /// <param name="notificationService"> The service used to create and manage notifications.</param>
        public class Handler(
            AppDbContext dbContext, 
            IImagesUploader<Animal> uploadService,
            INotificationService notificationService) 
            : IRequestHandler<Command, Result<string>>
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
            public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                request.Animal.ShelterId = request.ShelterId;
                dbContext.Animals.Add(request.Animal);
                
                var uploadResult = await uploadService.UploadImagesAsync(
                    request.Animal.Id,
                    request.Files,
                    request.Images,
                    cancellationToken
                );

                if (!uploadResult.IsSuccess)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<string>.Failure(uploadResult.Error ?? string.Empty, uploadResult.Code);
                }

                var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;
                if (!success)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<string>.Failure("Failed to save animal and images", 400);
                }

                await transaction.CommitAsync(cancellationToken);

                await NotifyUsers(request.Animal, cancellationToken);
                    
                return Result<string>.Success(request.Animal.Id, 201);
            }

            /// <summary>
            /// Notifies all users about the new animal added to the catalog.
            /// </summary>
            /// <param name="animal">The newly created animal.</param>
            /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
            /// <remarks>
            /// Broadcasts a notification to all users with the "User" role informing them
            /// that a new animal is available for adoption.
            /// This is a best-effort operation and does not affect the animal creation if notifications fail.
            /// </remarks>
            private async Task NotifyUsers(Animal animal, CancellationToken cancellationToken)
            {
                await notificationService.CreateAndSendToRoleAsync(
                    role: AppRoles.User,
                    NotificationType.NEW_ANIMAL_ADDED,
                    message: $"{animal.Name} é o novo patudo disponível para apadrinhamento e adoção responsável!",
                    animalId: animal.Id
                    );
            }
        }
    }
}