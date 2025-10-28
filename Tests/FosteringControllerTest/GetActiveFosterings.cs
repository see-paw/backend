using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Fosterings.Queries;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;

namespace Tests.Fosterings.Queries
{
    //codacy: ignore[complexity]
    public class GetActiveFosteringsTests
    {
        private readonly AppDbContext _context;
        private readonly GetActiveFosterings.Handler _handler;

        public GetActiveFosteringsTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _handler = new GetActiveFosterings.Handler(_context);
        }

        private async Task SeedFosteringsAsync(string userId)
        {
            var publicId = Guid.NewGuid().ToString();
            var animal1 = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Rex",
                Colour = "Brown",
                Cost = 50M,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                BirthDate = new DateOnly(2021, 5, 1),
                Sterilized = true,
                BreedId = Guid.NewGuid().ToString(),
                ShelterId = Guid.NewGuid().ToString(),
                AnimalState = AnimalState.Available,
                Images = new List<Image>
                {
                    new() { Id = Guid.NewGuid().ToString(), 
                        Url = "https://test/image1.jpg",  
                        Description = "Profile photo",
                        PublicId = publicId
                    }
                }
            };

            var animal2 = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Mimi",
                Colour = "White",
                Cost = 40M,
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                BirthDate = new DateOnly(2020, 3, 10),
                Sterilized = true,
                BreedId = Guid.NewGuid().ToString(),
                ShelterId = Guid.NewGuid().ToString(),
                AnimalState = AnimalState.Available
            };

            var activeFostering = new Fostering
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animal1.Id,
                Animal = animal1,
                UserId = userId,
                Status = FosteringStatus.Active,
                Amount = 15M,
                StartDate = DateTime.UtcNow.AddDays(-15)
            };

            var cancelledFostering = new Fostering
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animal2.Id,
                Animal = animal2,
                UserId = userId,
                Status = FosteringStatus.Cancelled,
                Amount = 20M,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(-10),
            };

            _context.Fosterings.AddRange(activeFostering, cancelledFostering);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task ReturnActiveFosteringsWhenTheyExist()
        {
            var userId = "user-1";
            await SeedFosteringsAsync(userId);

            var query = new GetActiveFosterings.Query { UserId = userId };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ShouldFail_WhenNoActiveFosteringsExist()
        {
            var userId = "user-2";
            var animal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Luna",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                BirthDate = new DateOnly(2020, 1, 1),
                BreedId = Guid.NewGuid().ToString(),
                ShelterId = Guid.NewGuid().ToString()
            };

            _context.Animals.Add(animal);
            _context.Fosterings.Add(new Fostering
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animal.Id,
                Animal = animal,
                UserId = userId,
                Status = FosteringStatus.Cancelled,
                Amount = 10M,
                StartDate = DateTime.UtcNow.AddDays(-20),
                EndDate = DateTime.UtcNow.AddDays(-5),
            });
            await _context.SaveChangesAsync();

            var query = new GetActiveFosterings.Query { UserId = userId };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task ShouldFail_WhenUserNeverHadFosteringsAtAll()
        {
            var query = new GetActiveFosterings.Query { UserId = "non-existent-user" };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.False(result.IsSuccess);
            
        }
    }
}
