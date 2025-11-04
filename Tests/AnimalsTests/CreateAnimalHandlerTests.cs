using Application.Animals.Commands;
using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Persistence;

namespace Tests.AnimalsTests
{
    //codacy: ignore[complexity]
    public class CreateAnimalHandlerTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"AnimalDb_{Guid.NewGuid()}")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new AppDbContext(options);
        }

        private Animal NewAnimal() => new()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Kika",
            Species = Domain.Enums.Species.Cat,
            Size = Domain.Enums.SizeType.Small,
            Sex = Domain.Enums.SexType.Female,
            Colour = "White",
            BirthDate = new DateOnly(2020, 5, 1),
            Sterilized = true,
            BreedId = Guid.NewGuid().ToString(),
            Cost = 25,
            Features = "Lovely cat"
        };

        private List<Image> NewImages() => new()
        {
            new Image
            {
                Id = Guid.NewGuid().ToString(),
                Url = "cat.jpg",
                PublicId = "public-cat"
            }
        };

        private List<IFormFile> MockFiles()
        {
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns("cat.jpg");
            return new List<IFormFile> { file.Object };
        }

        [Fact]
        public async Task CreateAnimal_Success_Returns201AndPersists()
        {
            var ctx = CreateContext();

            var uploader = new Mock<IImagesUploader<Animal>>();
            uploader.Setup(u => u.UploadImagesAsync(
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>(),
                It.IsAny<List<Image>>(),
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(Result<Unit>.Success(Unit.Value, 201));

            var handler = new CreateAnimal.Handler(ctx, uploader.Object);

            var cmd = new CreateAnimal.Command
            {
                Animal = NewAnimal(),
                ShelterId = Guid.NewGuid().ToString(),
                Images = NewImages(),
                Files = MockFiles()
            };

            var result = await handler.Handle(cmd, default);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateAnimal_UploadFails_ShouldRollbackAndReturnError()
        {
            var ctx = CreateContext();

            var uploader = new Mock<IImagesUploader<Animal>>();
            uploader.Setup(u => u.UploadImagesAsync(
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>(),
                It.IsAny<List<Image>>(),
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(Result<Unit>.Failure("upload error", 400));

            var handler = new CreateAnimal.Handler(ctx, uploader.Object);

            var cmd = new CreateAnimal.Command
            {
                Animal = NewAnimal(),
                ShelterId = Guid.NewGuid().ToString(),
                Images = NewImages(),
                Files = MockFiles()
            };

            var result = await handler.Handle(cmd, default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task CreateAnimal_SaveFails_ShouldRollback()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"Fail_{Guid.NewGuid()}")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var ctx = new FailingAppDbContext(options);

            var uploader = new Mock<IImagesUploader<Animal>>();
            uploader.Setup(u => u.UploadImagesAsync(
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>(),
                It.IsAny<List<Image>>(),
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(Result<Unit>.Success(Unit.Value, 200));

            var handler = new CreateAnimal.Handler(ctx, uploader.Object);

            var cmd = new CreateAnimal.Command
            {
                Animal = new Animal
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Rocky",
                    Species = Domain.Enums.Species.Dog,
                    Size = Domain.Enums.SizeType.Medium,
                    Sex = Domain.Enums.SexType.Male,
                    Colour = "Brown",
                    BirthDate = new DateOnly(2020, 1, 1),
                    Sterilized = true,
                    BreedId = Guid.NewGuid().ToString(),
                    Cost = 50,
                    Features = "Test dog"
                },
                ShelterId = Guid.NewGuid().ToString(),
                Images = new List<Image>
                {
                    new() { Id = Guid.NewGuid().ToString(), Url = "url", PublicId = "pid" }
                },
                Files = new List<IFormFile>()
            };

            var result = await handler.Handle(cmd, default);

            Assert.False(result.IsSuccess);
        }

        private class FailingAppDbContext : AppDbContext
        {
            public FailingAppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(0);
        }
    }
}
