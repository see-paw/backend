using Application.Core;
using Domain;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application.Interfaces;

public interface IImageAppService<T> where T : class, IHasPhotos
{
    Task<Result<Image>> AddImageAsync(
        AppDbContext dbContext,
        string entityId,
        IFormFile file,
        string description,
        bool isPrincipal,
        CancellationToken ct
    );

    Task<Result<Unit>> DeleteImageAsync(
        AppDbContext dbContext,
        string entityId,
        string publicId,
        CancellationToken ct
    );
}