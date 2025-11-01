using Application.Animals.Queries;
using Application.Core;
using Application.Shelters.Queries;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Tests.AnimalControllerTest.cs
{


    //codacy: ignore[complexity]
    public class GetAnimalListTests
    {
        // ===== Helper method =====
        // Creates an isolated in-memory AppDbContext for testing.
        private AppDbContext CreateInMemoryContext(List<Animal> animals)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;

            var context = new AppDbContext(options);

            // Add required related entities
            var breed = new Breed
            {
                Id = "1a1a1111-1111-1111-1111-111111111111",
                Name = "Rafeiro"
            };

            var shelter = new Shelter
            {
                Id = "11111111-1111-1111-1111-111111111111",
                Name = "Animais de Rua"
            };

            context.Breeds.Add(breed);
            context.Shelters.Add(shelter);
            context.Animals.AddRange(animals);
            context.SaveChanges();

            return context;
        }

        // ===== Helper method =====
        // Creates a fully valid Animal for testing purposes.
        private Animal CreateAnimal(string name, AnimalState state = AnimalState.Available)
        {
            return new Animal
            {
                Name = name,
                AnimalState = state,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Brown",
                BirthDate = new DateOnly(2020, 1, 1),
                Sterilized = true,
                BreedId = "1a1a1111-1111-1111-1111-111111111111",
                Cost = 100m,
                Features = "Friendly",
                CreatedAt = DateTime.UtcNow,
                ShelterId = "11111111-1111-1111-1111-111111111111"
            };
        }

        // ===== Tests =====

        // Test: Ensure all animals are returned when page number is valid.

        [Fact]
        public async Task ReturnAllAnimalsWithPageNumberValid()
        {
            var animals = new List<Animal>
            {
                CreateAnimal("Charlie"),
                CreateAnimal("Buddy")
            };
            var context = CreateInMemoryContext(animals);
            var handler = new GetAnimalList.Handler(context);

            var query = new GetAnimalList.Query { PageNumber = 1 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal(2, result.Value.Count);
        }

        // Test: Ensure only animals with Available or PartiallyFostered states are returned.
        [Fact]
        public async Task OnlyAvailableAndPartiallyFostered()
        {
            var animals = new List<Animal>
            {
                CreateAnimal("Charlie", AnimalState.Available),
                CreateAnimal("Buddy", AnimalState.PartiallyFostered),
                CreateAnimal("Rex", AnimalState.HasOwner) // Should be excluded
            };

            var context = CreateInMemoryContext(animals);
            var handler = new GetAnimalList.Handler(context);
            var query = new GetAnimalList.Query { PageNumber = 1 }; //valid page number

            var result = await handler.Handle(query, CancellationToken.None);


            Assert.Equal(2, result.Value.Count);
        }

        // Test: Ensure animals are returned in alphabetical order by name.

        [Fact]
        public async Task AnimalsInAlphabeticalOrder()
        {
            var animals = new List<Animal>
            {
                CreateAnimal("Zara"),
                CreateAnimal("Bella")
            };

            var context = CreateInMemoryContext(animals);
            var handler = new GetAnimalList.Handler(context);
            var query = new GetAnimalList.Query { PageNumber = 1 };

            var result = await handler.Handle(query, CancellationToken.None);

            var ordered = result.Value.ToList();
            Assert.Equal("Bella", ordered[0].Name);
            Assert.Equal("Zara", ordered[1].Name);
        }

        // Test: Ensure failure is returned when no animals exist.

        [Fact]
        public async Task NoAnimalsExist()
        {
            var context = CreateInMemoryContext(new List<Animal>());
            var handler = new GetAnimalList.Handler(context);
            var query = new GetAnimalList.Query { PageNumber = 1 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal("No animals found", result.Error);
        }

        // Test: Ensure pagination works correctly when multiple pages of results exist.
        [Fact]
        public async Task PaginateResultsCorrectly()
        {
            var animals = Enumerable.Range(1, 30)
                .Select(i => CreateAnimal($"Animal{i}"))
                .ToList();

            var context = CreateInMemoryContext(animals);
            var handler = new GetAnimalList.Handler(context);

            var query = new GetAnimalList.Query { PageNumber = 2 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal(10, result.Value.Count);
        }

        // Test: Ensure total pages are calculated correctly when multiple pages of results exist.
        [Fact]
        public async Task ShouldCalculateTotalPagesCorrectly()
        {
            var animals = Enumerable.Range(1, 30)
                .Select(i => CreateAnimal($"Animal{i}"))
                .ToList();

            var context = CreateInMemoryContext(animals);
            var handler = new GetAnimalList.Handler(context);

            var query = new GetAnimalList.Query { PageNumber = 1 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal(2, result.Value.TotalPages);
        }
    }
}
