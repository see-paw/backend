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
        FosteringDomainService fosteringDomainService,
        IFosteringService fosteringService,
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