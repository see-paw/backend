using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Fosterings.Commands;

public class AddFostering
{
    public class Command : IRequest<Result<Fostering>>
    {
        public required string AnimalId { get; set; }
        public decimal MonthValue { get; set; }
    }
    
    public class Handler(AppDbContext dbContext, 
        FosteringService fosteringService,
        IUserAccessor userAccessor) : IRequestHandler<Command, Result<Fostering>>
    {
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

            if (animal.AnimalState is AnimalState.Inactive 
                or AnimalState.TotallyFostered 
                or AnimalState.HasOwner)
            {
                var (message, code) = animal.AnimalState switch
                {
                    AnimalState.Inactive => ("Animal is inactive", 409),
                    AnimalState.TotallyFostered => ("Animal is totally fostered", 409),
                    AnimalState.HasOwner => ("Animal has an owner, not available for fostering", 409),
                    _ => ("Invalid animal state", 400)
                };

                return Result<Fostering>.Failure(message, code);
            }
            
            var user = await userAccessor.GetUserAsync();

            if (fosteringService.IsAlreadyFosteredByUser(animal, user.Id))
            {
                await transaction.RollbackAsync(ct);
                return Result<Fostering>.Failure("You already foster this animal", 409);
            }
            
            var newSupport = fosteringService.GetAnimalCurrentSupport(animal) + request.MonthValue;

            if (newSupport > animal.Cost)
            {
                await transaction.RollbackAsync(ct);
                return Result<Fostering>.Failure("Monthly value surpasses animal costs", 422);
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
            
            fosteringService.UpdateFosteringState(animal);
            
            dbContext.Fosterings.Add(newFostering);
            
            await dbContext.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
            
            return Result<Fostering>.Success(newFostering, 201);
        }
    }
}