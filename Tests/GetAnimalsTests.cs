using API.Controllers;
using API.Core;
using AutoMapper;
using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

public class GetAnimalsTests
{
    // ===== Helper method =====
    // Creates an AnimalsController with an in-memory database.
    // This avoids the need for a real SQL Server and makes tests fast and isolated.
    private AppDbContext CreateInMemoryContext(List<Animal> animals)
    {
        // Create options using an InMemory database.
        // Guid.NewGuid() ensures each test uses a unique database name, so data from one test cannot leak into another.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        // Instantiate the context with these options.
        var context = new AppDbContext(options);

        // Preload any test data (if provided).
        context.Animals.AddRange(animals);

        // Save to the in-memory provider.
        context.SaveChanges();


        return context;
    }

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

    private Animal CreateInvalidAnimal(string name, string shelterId)
    {
        return new Animal
        {
            Name = name,
            AnimalState = AnimalState.HasOwner,
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

    // When pageNumber < 1, the controller should return BadRequest with the message "Page number must be 1 or greater".
    [Fact]
    public async Task GetAnimalsWithPageNumberLessThanOne()
    {

        var animals = new List<Animal> { };//empty database
        var context = CreateInMemoryContext(animals);
        var mapperMock = new Mock<IMapper>();
        //the 'controller' variable holds a fully functional AnimalsController object
        var controller = new AnimalsController(context, mapperMock.Object);

        var result = await controller.GetAnimals(0);// invalid page number

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page number must be 1 or greater", badRequest.Value);
    }

    // When there are no animals in the database, the controller should return NotFound with the message "No animals found".
    [Fact]
    public async Task GetAnimalsWithNoAnimalsFound()
    {
        var animals = new List<Animal> { };// empty database
        var context = CreateInMemoryContext(animals);
        var mapperMock = new Mock<IMapper>();
        var controller = new AnimalsController(context, mapperMock.Object);

        var result = await controller.GetAnimals(1);// valid page number

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("No animals found", notFound.Value);
    }

    // When there are animals available, it should return Ok with a PagedList<Animal>.
    [Fact]
    public async Task GetAnimals()
    {
        var shelterId = Guid.NewGuid().ToString();
        var animals = new List<Animal>
            {
                CreateValidAnimal("Charlie", shelterId),
                CreateValidAnimal("Buddy", shelterId)
            };
       
        var context = CreateInMemoryContext(animals);
        var mapperMock = new Mock<IMapper>();
        var controller = new AnimalsController(context, mapperMock.Object);

        // This method returns an ActionResult<PagedList<Animal>>, meaning it wraps both the HTTP result (e.g. Ok, NotFound)  and the returned value (a PagedList<Animal>).
        var result = await controller.GetAnimals(1);

        //The.Result property contains the HTTP response object. It is expected to be of type OkObjectResult (HTTP 200 OK).
        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        //Check that the body of the response(okResult.Value) is a PagedList < Animal >
        var pagedList = Assert.IsType<PagedList<Animal>>(okResult.Value);

        //Verify that the paged list contains exactly 2 items
        Assert.Equal(2, pagedList.Count);
    }

    [Fact]
    public async Task GetAnimalsWithOnlyAvailableAndPartiallyFostered()
    {
        var shelterId = Guid.NewGuid().ToString();
        var animals = new List<Animal>
            {
                CreateValidAnimal("Charlie", shelterId),
                CreateValidAnimal("Buddy", shelterId),
                CreateInvalidAnimal("Rex", shelterId) // This animal has state "HasOwner" and should be excluded

            };

        var context = CreateInMemoryContext(animals);
        var mapperMock = new Mock<IMapper>();
        var controller = new AnimalsController(context, mapperMock.Object);

        var result = await controller.GetAnimals(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pagedList = Assert.IsType<PagedList<Animal>>(okResult.Value);

        // Should only include available and partially fostered animals
        Assert.All(pagedList, a =>
            Assert.True(a.AnimalState == AnimalState.Available || a.AnimalState == AnimalState.PartiallyFostered));

        // Should NOT include "Rex" because he is "HasOwner"
        Assert.DoesNotContain(pagedList, a => a.Name == "Rex");
    }

    [Fact]
    public async Task GetOrderedAnimals()
    {

        var shelterId = Guid.NewGuid().ToString();
        var animals = new List<Animal>
            {
                CreateValidAnimal("Zara", shelterId),
                CreateValidAnimal("Bella", shelterId)
            };

        var context = CreateInMemoryContext(animals);
        var mapperMock = new Mock<IMapper>();
        var controller = new AnimalsController(context, mapperMock.Object);

        var result = await controller.GetAnimals(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pagedList = Assert.IsType<PagedList<Animal>>(okResult.Value);

        // Expect alphabetical order: "Bella" first, "Zara" second
        Assert.Collection(pagedList,
            a => Assert.Equal("Bella", a.Name),
            a => Assert.Equal("Zara", a.Name));
    }

}
