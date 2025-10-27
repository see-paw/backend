using Application.Core;
using Application.Images.Services;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application.Images.Commands;

public class AddImage<T> where T : class, IHasPhotos
{
    public class Command: IRequest<Result<Image>>
    {
        public required IFormFile File { get; set; }
        public required string Description { get; set; }
        public required bool IsPrincipal { get; set; }

        public required string EntityId { get; set; }
    }
    
    public class Handler(AppDbContext dbContext, 
        IImageService imageService,
        IImageOwnerLoader<T> loader,
        IPrincipalImageEnforcer principalEnforcer,
        IImageOwnerLinker<T> linker): IRequestHandler<Command, Result<Image>>
    {
        public async Task<Result<Image>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var entityId = request.EntityId;
                var entity = await loader.LoadAsync(dbContext, entityId, cancellationToken);
                
                var folder = $"SeePaw/{typeof(T).Name}/{request.EntityId}";
            
                var uploadResult = await imageService.UploadImage(request.File, folder);

                if (uploadResult == null)
                {
                    return Result<Image>.Failure("Failed to upload photo", 400);
                }
            
                var img = new Image
                {
                    Url = uploadResult.Url,
                    PublicId = uploadResult.PublicId,
                    Description = request.Description,
                    IsPrincipal = request.IsPrincipal
                };
            
                linker.Link(entity, img, entityId);
            
                principalEnforcer.EnforceSinglePrincipal(entity.Images, img);
            
                var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;
                return !result ? Result<Image>.Failure("Problem uploading photo", 400) : Result<Image>.Success(img, 201);
            }
            catch (KeyNotFoundException)
            {
                return Result<Image>.Failure("Entity not found to add Image to.", 404);
            }
            catch (ArgumentException ex)
            {
                return Result<Image>.Failure(ex.Message, 400);
            }
        }
    }
}