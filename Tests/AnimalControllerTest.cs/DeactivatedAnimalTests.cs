using Application.Animals.Commands;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;

namespace Tests.Animals
{
    //codacy: ignore[complexity]
    public class DeactivateAnimalTests
    {
        private readonly AppDbContext _context;

        public DeactivateAnimalTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
        }

        private async Task<Animal> SeedAnimalAsync(AnimalState state, string shelterId)
        {
            var animal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Rocky",
                Colour = "Brown",
                Cost = 100,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                BirthDate = new DateOnly(2020, 1, 1),
                Sterilized = true,
                BreedId = Guid.NewGuid().ToString(),
                ShelterId = shelterId,
                AnimalState = state
            };

            _context.Animals.Add(animal);
            await _context.SaveChangesAsync();
            return animal;
        }

        [Fact]
        public async Task DeactivateAnimal_WhenShelterDoesNotExist()
        {
            var handler = new DeactivateAnimal.Handler(_context);
            var result = await handler.Handle(new DeactivateAnimal.Command
            {
                AnimalId = Guid.NewGuid().ToString(),
                ShelterId = "non-existent"
            }, default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task DeactivateAnimal_WhenAnimalDoesNotExist()
        {
            var shelterId = "22222222-2222-2222-2222-222222222222";
            _context.Shelters.Add(new Shelter { Id = shelterId, Name = "Test Shelter" });
            await _context.SaveChangesAsync();

            var handler = new DeactivateAnimal.Handler(_context);
            var result = await handler.Handle(new DeactivateAnimal.Command
            {
                AnimalId = "invalid-id",
                ShelterId = shelterId
            }, default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task DeactivateAnimal_WhenAnimalHasOwner()
        {
            var shelterId = "22222222-2222-2222-2222-222222222222";
            _context.Shelters.Add(new Shelter { Id = shelterId, Name = "Test Shelter" });
            await _context.SaveChangesAsync();

            var animal = await SeedAnimalAsync(AnimalState.HasOwner, shelterId);

            var handler = new DeactivateAnimal.Handler(_context);
            var result = await handler.Handle(new DeactivateAnimal.Command
            {
                AnimalId = animal.Id,
                ShelterId = shelterId
            }, default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task DeactivateAnimal_WhenAnimalIsPartiallyFostered()
        {
            var shelterId = "22222222-2222-2222-2222-222222222222";
            _context.Shelters.Add(new Shelter { Id = shelterId, Name = "Test Shelter" });
            await _context.SaveChangesAsync();

            var animal = await SeedAnimalAsync(AnimalState.PartiallyFostered, shelterId);

            var handler = new DeactivateAnimal.Handler(_context);
            var result = await handler.Handle(new DeactivateAnimal.Command
            {
                AnimalId = animal.Id,
                ShelterId = shelterId
            }, default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task DeactivateAnimal_WhenAnimalIsTotallyFostered()
        {
            var shelterId = "22222222-2222-2222-2222-222222222222";
            _context.Shelters.Add(new Shelter { Id = shelterId, Name = "Test Shelter" });
            await _context.SaveChangesAsync();

            var animal = await SeedAnimalAsync(AnimalState.TotallyFostered, shelterId);

            var handler = new DeactivateAnimal.Handler(_context);
            var result = await handler.Handle(new DeactivateAnimal.Command
            {
                AnimalId = animal.Id,
                ShelterId = shelterId
            }, default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task DeactivateAnimal_WhenAnimalIsAvailable()
        {
            var shelterId = "22222222-2222-2222-2222-222222222222";
            _context.Shelters.Add(new Shelter { Id = shelterId, Name = "Test Shelter" });
            await _context.SaveChangesAsync();

            var animal = await SeedAnimalAsync(AnimalState.Available, shelterId);

            var handler = new DeactivateAnimal.Handler(_context);
            var result = await handler.Handle(new DeactivateAnimal.Command
            {
                AnimalId = animal.Id,
                ShelterId = shelterId
            }, default);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task DeactivateAnimal_ShouldSucceed_WhenAnimalIsAlreadyInactive()
        {
            var shelterId = "22222222-2222-2222-2222-222222222222";
            _context.Shelters.Add(new Shelter { Id = shelterId, Name = "Test Shelter" });
            await _context.SaveChangesAsync();

            var animal = await SeedAnimalAsync(AnimalState.Inactive, shelterId);

            var handler = new DeactivateAnimal.Handler(_context);
            var result = await handler.Handle(new DeactivateAnimal.Command
            {
                AnimalId = animal.Id,
                ShelterId = shelterId
            }, default);

            Assert.True(result.IsSuccess);
        }
    }
}
