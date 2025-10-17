using API.Controllers;
using API.Core;
using AutoMapper;
using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests;
public class GetAnimalsTests
{

    // ===== Helper method =====
    // Creates an AnimalsController with an in-memory database.
    // This avoids the need for a real SQL Server and makes tests fast and isolated.
    private AppDbContext CreateInMemoryContext(Shelter? shelter = null, List<Animal>? animals = null)
    {
        // Create in-memory database options.
        // Each test uses a unique database name (via Guid) so they never share data.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
            .Options;

        // Create the in-memory context.
        var context = new AppDbContext(options);

        // Add a shelter if provided.
        if (shelter != null)
        {
            context.Shelters.AddRange(shelter);
        }

        // Add animal data if provided.
        if (animals != null)
        {
            context.Animals.AddRange(animals);
        }

        // Save all changes to the in-memory store.
        context.SaveChanges();

        // Return the ready-to-use context.
        return context;
    }

    // ===== Helper method =====
    // Creates a valid shelter that satisfies all validation rules from the Shelter entity.
    private Shelter CreateValidShelter(string shelterId)
    {
        return new Shelter
        {
            ShelterId = shelterId,
            Name = "Happy Paws Shelter",
            Street = "123 Animal Street",
            City = "Porto",
            PostalCode = "4000-123",
            Phone = "912345678",
            NIF = "123456789",
            MainImageUrl = "https://example.com/shelter.jpg",
            OpeningTime = new TimeSpan(9, 0, 0),
            ClosingTime = new TimeSpan(18, 0, 0),
            CreatedAt = DateTime.UtcNow
        };
    }

    // ===== Helper method =====
    // Creates a fully valid Animal for testing purposes.
    private Animal CreateValidAnimal(string name, string shelterId)
    {
        return new Animal
        {
            Name = name,
            AnimalState = AnimalState.Available,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Breed = Breed.GoldenRetriever,
            Cost = 100m,
            Features = "Test animal",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            MainImageUrl = "https://example.com/animal.jpg",
            ShelterId = shelterId
        };
    }

    // When the shelter does not exist but the page number is valid, the controller should return NotFound with the message "Shelter not found".
    [Fact]
    public async Task GetAnimalsByShelterWhenShelterDoesNotExistButPageNumberIsValid()
    {
        var animals = new List<Animal> { };// empty database
        var context = CreateInMemoryContext(null, animals); 
        var controller = new SheltersController(context);
        var nonExistentShelterId = Guid.NewGuid().ToString(); // Use a random, non-existent shelter ID

        var result = await controller.GetAnimalsByShelter(nonExistentShelterId, 1);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Shelter not found", notFound.Value);
    }

    // When the shelter exists but has no animals, it should return NotFound with the message "No animals found for this shelter".
    [Fact]
    public async Task GetAnimalsByShelterWhenNoAnimalsExistButPageNumberIsValid()
    {
        var animals = new List<Animal> { };// empty database
        var shelterId = Guid.NewGuid().ToString();
        var shelter = CreateValidShelter(shelterId);

        var context = CreateInMemoryContext(shelter, animals);
        var controller = new SheltersController(context);

        var result = await controller.GetAnimalsByShelter(shelterId, 1);//page number valid

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("No animals found for this shelter", notFound.Value);
    }


    // When animals exist for a valid shelter and valid page number,the controller should return Ok with a PagedList<Animal>.
    [Fact]
    public async Task GetAnimalsByShelterWithAnimalsAndPageNumberValid()
    {
        // Arrange
        var shelterId = Guid.NewGuid().ToString();
        var shelter = CreateValidShelter(shelterId);
        var animals = new List<Animal>
            {
                CreateValidAnimal("Charlie", shelterId),
                CreateValidAnimal("Buddy", shelterId)
            };

        var context = CreateInMemoryContext(shelter, animals);
        var controller = new SheltersController(context);

        var result = await controller.GetAnimalsByShelter(shelterId, 1);//page number valid

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pagedList = Assert.IsType<PagedList<Animal>>(okResult.Value);

        // Expect 2 results in the list
        Assert.Equal(2, pagedList.Count);
    }

    //When animals exist, they should be returned in alphabetical order.
    [Fact]
    public async Task GetAnimalsByShelterWithAnimalsOrderedAndWithPageNumberValid()
    {
        var shelterId = Guid.NewGuid().ToString();
        var shelter = CreateValidShelter(shelterId);

        // Create two valid animals belonging to the same shelter.
        var animals = new List<Animal>
            {
                CreateValidAnimal("Charlie", shelterId),
                CreateValidAnimal("Buddy", shelterId)
            };

        var context = CreateInMemoryContext(shelter, animals);
        var controller = new SheltersController(context);

        var result = await controller.GetAnimalsByShelter(shelterId, 1);//page number valid

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pagedList = Assert.IsType<PagedList<Animal>>(okResult.Value);

        // Ensure correct alphabetical order
        Assert.Collection(pagedList,
                a => Assert.Equal("Buddy", a.Name),
                a => Assert.Equal("Charlie", a.Name));
    }


    // When pageNumber < 1, the controller should return BadRequest with the message "Page number must be 1 or greater".
    [Fact]
    public async Task GetAnimalsWithPageNumberLessThanOne()
    {

        var animals = new List<Animal> { };//empty database
        var shelterId = Guid.NewGuid().ToString();
        var shelter = CreateValidShelter(shelterId);
        var context = CreateInMemoryContext(shelter, animals);
      
        var controller = new SheltersController(context);

        var result = await controller.GetAnimalsByShelter(shelterId, 0);// invalid page number

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page number must be 1 or greater", badRequest.Value);
    }

    // When a valid shelter exists and has animals,  but the requested page number is greater than the total number of pages,
    // the controller should return NotFound with the message "No animals found for this shelter".
    [Fact]
    public async Task GetAnimalsByShelterWhenPageNumberBeyondTotalPages()
    {
        var shelterId = Guid.NewGuid().ToString();
        var shelter = CreateValidShelter(shelterId);

        var animals = new List<Animal>
    {
        CreateValidAnimal("Buddy", shelterId),
        CreateValidAnimal("Charlie", shelterId)
    };

        var context = CreateInMemoryContext(shelter, animals);
        var controller = new SheltersController(context);

        var result = await controller.GetAnimalsByShelter(shelterId, 2);// request page 2 even though there is only one page of data

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("No animals found for this shelter", notFound.Value);
    }

}




