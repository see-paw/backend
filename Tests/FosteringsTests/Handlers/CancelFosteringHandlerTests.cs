using Application.Fosterings.Commands;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Xunit;

namespace Tests.FosteringsTests.Handlers
{
    // codacy: ignore[complexity]
    public class CancelFosteringHandlerTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IFosteringService> _fosteringService; 

        public CancelFosteringHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _fosteringService = new Mock<IFosteringService>(); 
        }

        private async Task<Fostering> SeedFosteringAsync(FosteringStatus status, string userId)
        {
            var animal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Milo",
                Colour = "Black",
                Cost = 50,
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Male,
                BirthDate = new DateOnly(2021, 1, 1),
                Sterilized = true,
                BreedId = Guid.NewGuid().ToString(),
                ShelterId = Guid.NewGuid().ToString(),
                AnimalState = AnimalState.Available,
                Fosterings = new List<Fostering>() 
            };

            var fostering = new Fostering
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animal.Id,
                Animal = animal,              
                UserId = userId,
                Status = status,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = null,
                Amount = 20.0M
            };

            animal.Fosterings.Add(fostering); 

            _context.Animals.Add(animal);
            _context.Fosterings.Add(fostering);
            await _context.SaveChangesAsync();
            return fostering;
        }

        [Fact]
        public async Task CancelFostering_ShouldFail_WhenRecordDoesNotExist()
        {
            var handler = new CancelFostering.Handler(_context, _fosteringService.Object); 

            var result = await handler.Handle(new CancelFostering.Command
            {
                FosteringId = Guid.NewGuid().ToString(),
                UserId = "user-1"
            }, default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task CancelFostering_ShouldFail_WhenRecordBelongsToDifferentUser()
        {
            var userId = "user-1";
            var otherUser = "user-2";

            var fostering = await SeedFosteringAsync(FosteringStatus.Active, userId);

            var handler = new CancelFostering.Handler(_context, _fosteringService.Object); 
            var result = await handler.Handle(new CancelFostering.Command
            {
                FosteringId = fostering.Id,
                UserId = otherUser
            }, default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task CancelFostering_ShouldFail_WhenStatusIsCancelled()
        {
            var userId = "user-1";
            var fostering = await SeedFosteringAsync(FosteringStatus.Cancelled, userId);

            var handler = new CancelFostering.Handler(_context, _fosteringService.Object); 
            var result = await handler.Handle(new CancelFostering.Command
            {
                FosteringId = fostering.Id,
                UserId = userId
            }, default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task CancelFostering_ShouldFail_WhenStatusIsTerminated()
        {
            var userId = "user-1";
            var fostering = await SeedFosteringAsync(FosteringStatus.Terminated, userId);

            var handler = new CancelFostering.Handler(_context, _fosteringService.Object); 
            var result = await handler.Handle(new CancelFostering.Command
            {
                FosteringId = fostering.Id,
                UserId = userId
            }, default);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task CancelFostering_ShouldSucceed_WhenStatusIsActive()
        {
            var userId = "user-1";
            var fostering = await SeedFosteringAsync(FosteringStatus.Active, userId);

            var handler = new CancelFostering.Handler(_context, _fosteringService.Object); 
            var result = await handler.Handle(new CancelFostering.Command
            {
                FosteringId = fostering.Id,
                UserId = userId
            }, default);

            Assert.True(result.IsSuccess);

            //to make sure the service was called
            _fosteringService.Verify(s => s.UpdateFosteringState(It.IsAny<Animal>()), Times.Once);
        }
    }
}
