using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Fosterings.Commands;


/// <summary>
/// Command that creates a new <see cref="Fostering"/> record, allowing a user to sponsor an animal.
/// </summary>
public class AddFostering
{
    /// <summary>
    /// Represents the command containing the necessary data to create a new <see cref="Fostering"/>.
    /// </summary>
    public class Command : IRequest<Result<Fostering>>
    {
        /// <summary>
        /// The unique identifier (GUID) of the animal to be fostered.
        /// </summary>
        public required string AnimalId { get; set; }
        
        /// <summary>
        /// The monthly contribution value provided by the user.
        /// Must be greater than or equal to the configured minimum
        /// and cannot exceed the animal’s total cost.
        /// </summary>
        public decimal MonthValue { get; set; }
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Handler"/> class.
    /// </summary>
    /// <param name="dbContext">The database context of the application.</param>
    /// <param name="fosteringDomainService">The domain service containing fostering business logic.</param>
    /// <param name="fosteringService">The application service responsible for updating the animal’s fostering state.</param>
    /// <param name="userAccessor">Provides access to the currently authenticated user.</param>
    public class Handler(AppDbContext dbContext, 
        FosteringDomainService fosteringDomainService,
        IFosteringService fosteringService,
        IUserAccessor userAccessor) : IRequestHandler<Command, Result<Fostering>>
    {
        /// <summary>
        /// Executes the command to create a new fostering record.
        /// </summary>
        /// <param name="request">The command containing the animal ID and monthly contribution.</param>
        /// <param name="ct">The cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> object containing:
        /// <list type="bullet">
        /// <item><description>The created <see cref="Fostering"/> record on success (201 Created).</description></item>
        /// <item><description>An error message with code 404 if the animal is not found.</description></item>
        /// <item><description>An error message with code 409 if the animal is in an invalid state or already fostered by the same user.</description></item>
        /// <item><description>An error message with code 422 if the total monthly support exceeds the animal’s cost.</description></item>
        /// </list>
        /// </returns>
        public async Task<Result<Fostering>> Handle(Command request, CancellationToken ct)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
            
            var animal = await dbContext.Animals
                .Include(a => a.Fosterings
                    .Where(f => f.Status == FosteringStatus.Active))
                .FirstOrDefaultAsync(a => a.Id == request.AnimalId, ct);
            
            if (animal == null)
            {
                await transaction.RollbackAsync(ct);
                return Result<Fostering>.Failure("Animal not found", 404);
            }

            var result = fosteringService.isInValidStateForFostering(animal);
            
            if (!result.IsSuccess)
            {
                await transaction.RollbackAsync(ct);
                return Result<Fostering>.Failure(result.Error ?? string.Empty, result.Code);
            }
            
            var user = await userAccessor.GetUserAsync();

            if (fosteringDomainService.IsAlreadyFosteredByUser(animal, user.Id))
            {
                await transaction.RollbackAsync(ct);
                return Result<Fostering>.Failure("You already foster this animal", 409);
            }

            var newFostering = new Fostering
            {
                AnimalId = animal.Id,
                UserId = user.Id,
                Amount = request.MonthValue,
                Animal = animal,
                User = user,
            };

            animal.Fosterings.Add(newFostering);

            try
            {
                fosteringService.UpdateFosteringState(animal);
            }
            catch (InvalidOperationException e)
            {
                await transaction.RollbackAsync(ct);
                return Result<Fostering>.Failure(e.Message, 422);
            }
            
            dbContext.Fosterings.Add(newFostering);
            
            await dbContext.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
            
            return Result<Fostering>.Success(newFostering, 201);
        }
    }
}