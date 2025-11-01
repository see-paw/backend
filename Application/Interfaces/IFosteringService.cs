using Application.Core;
using Domain;
using MediatR;

namespace Application.Interfaces;

public interface IFosteringService
{
    public void UpdateFosteringState(Animal animal);
    public Result<Unit> isInValidStateForFostering(Animal animal);
}