using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application.Images.Commands;

public class AddImagesAnimal
{
    public class Command : IRequest<Result<List<Image>>>
    {
        public required string AnimalId { get; set; }
        
        public required List<Image> Images { get; set; }
        
        public required List<IFormFile> Files { get; set; }
    }

    public class Handler(AppDbContext dbContext, IImageAppService<Animal> imageAppService) : IRequestHandler<Command, Result<List<Image>>>
    {
        public async Task<Result<List<Image>>> Handle(Command request, CancellationToken ct)
        {
            if (request.Files.Count != request.Images.Count)
                return Result<List<Image>>.Failure("Mismatch between files and image metadata.", 400);

            var animal = await dbContext.Animals.FindAsync([request.AnimalId], ct);

            if (animal == null)
                return Result<List<Image>>.Failure("Animal not found", 404);

            var resultImages = new List<Image>();
            
            for (var i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];
                var meta = request.Images[i];

                var imgResult = await imageAppService.AddImageAsync(
                    dbContext,
                    animal.Id,
                    file,
                    meta.Description ?? string.Empty,
                    meta.IsPrincipal,
                    ct
                );

                if (!imgResult.IsSuccess)
                {
                    return Result<List<Image>>.Failure($"Image upload failed: {imgResult.Error}", 400);
                }

                if (imgResult.Value != null)
                {
                    resultImages.Add(imgResult.Value);
                }
                else
                {
                    return Result<List<Image>>.Failure("Image upload returned null value.", 500);
                }
            }
            return Result<List<Image>>.Success(resultImages, 201);
        }
    }
}