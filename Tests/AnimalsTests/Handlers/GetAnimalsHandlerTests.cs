using Application.Animals.Queries;
using Application.Core;
using Application.Shelters.Queries;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Tests.AnimalsTests.Handlers
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

            Assert.Equal(2, result.Value.Items.Count);
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


            Assert.Equal(2, result.Value.Items.Count);
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

            Assert.Equal(10, result.Value.Items.Count);
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

        //Test: should order by name ascending 
        [Fact]
        public async Task ShouldOrderByNameAscending()
        {
            var animals = new List<Animal>
            {
                CreateAnimal("Zara"),
                CreateAnimal("Bella")
            };

            var context = CreateInMemoryContext(animals);
            var handler = new GetAnimalList.Handler(context);

            var query = new GetAnimalList.Query { SortBy = "name", Order = "asc" };

            var result = await handler.Handle(query, CancellationToken.None);

            var list = result.Value.Items.ToList();
            Assert.Equal("Bella", list[0].Name);
            Assert.Equal("Zara", list[1].Name);
        }


        //Test: should order by name descending
        [Fact]
        public async Task ShouldOrderByNameDescending()
        {
            var animals = new List<Animal>
            {
                CreateAnimal("Zara"),
                CreateAnimal("Bella")
            };

            var context = CreateInMemoryContext(animals);
            var handler = new GetAnimalList.Handler(context);

            var query = new GetAnimalList.Query { SortBy = "name", Order = "desc" };

            var result = await handler.Handle(query, CancellationToken.None);

            var list = result.Value.Items.ToList();
            Assert.Equal("Zara", list[0].Name);
            Assert.Equal("Bella", list[1].Name);
        }

        //Test: should order by age ascending 
        [Fact]
        public async Task ShouldOrderByAgeAscending()
        {
            var animalYoung = CreateAnimal("Puppy");
            animalYoung.BirthDate = new DateOnly(2023, 1, 1);

            var animalOld = CreateAnimal("Senior");
            animalOld.BirthDate = new DateOnly(2010, 1, 1);

            var context = CreateInMemoryContext(new List<Animal> { animalOld, animalYoung });
            var handler = new GetAnimalList.Handler(context);

            var query = new GetAnimalList.Query { SortBy = "age", Order = "asc" };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal("Puppy", result.Value.Items.First().Name); // younger animal first
        }

        //Test: should order by age descending
        [Fact]
        public async Task ShouldOrderByAgeDescending()
        {
            var animalYoung = CreateAnimal("Puppy");
            animalYoung.BirthDate = new DateOnly(2023, 1, 1);

            var animalOld = CreateAnimal("Senior");
            animalOld.BirthDate = new DateOnly(2010, 1, 1);

            var context = CreateInMemoryContext(new List<Animal> { animalOld, animalYoung });
            var handler = new GetAnimalList.Handler(context);

            var query = new GetAnimalList.Query { SortBy = "age", Order = "asc" };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal("Senior", result.Value.Items.Last().Name); // oldest animal first
        }

        //Test: should order by created at descending (is also the default case)
        [Fact]
        public async Task ShouldOrderByCreatedAtDescendingByDefault()
        {
            var animal1 = CreateAnimal("Angelo");
            animal1.CreatedAt = DateTime.UtcNow.AddHours(-5);

            var animal2 = CreateAnimal("Bina");
            animal2.CreatedAt = DateTime.UtcNow;

            var context = CreateInMemoryContext(new List<Animal>() { animal1, animal2 });
            var handler = new GetAnimalList.Handler(context);

            var result = await handler.Handle(new GetAnimalList.Query(), CancellationToken.None);

            Assert.Equal("Bina", result.Value.Items.First().Name); // the most recently created animal first
        }

        //Test: should order by created at ascending
        [Fact]
        public async Task ShouldOrderByCreatedAtAscendingByDefault()
        {
            var animal1 = CreateAnimal("Angelo");
            animal1.CreatedAt = DateTime.UtcNow.AddHours(-5);

            var animal2 = CreateAnimal("Bina");
            animal2.CreatedAt = DateTime.UtcNow;

            var context = CreateInMemoryContext(new List<Animal>() { animal1, animal2 });
            var handler = new GetAnimalList.Handler(context);

            var result = await handler.Handle(new GetAnimalList.Query(), CancellationToken.None);

            Assert.Equal("Angelo", result.Value.Items.Last().Name); // the least recently created animal last
        }



    }
}
