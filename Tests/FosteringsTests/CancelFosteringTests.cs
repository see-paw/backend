using Application.Fosterings.Commands;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;

namespace Tests.FosteringControllerTest
{
    // codacy: ignore[complexity]
    public class CancelFosteringTests
    {
        private readonly AppDbContext _context;

        public CancelFosteringTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
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
                AnimalState = AnimalState.Available
            };

            var fostering = new Fostering
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animal.Id,
                UserId = userId,
                Status = status,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = null,
                Amount = 20.0M
            };

            _context.Animals.Add(animal);
            _context.Fosterings.Add(fostering);
            await _context.SaveChangesAsync();
            return fostering;
        }

        [Fact]
        public async Task CancelFostering_ShouldFail_WhenRecordDoesNotExist()
        {
            var handler = new CancelFostering.Handler(_context);

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

            var handler = new CancelFostering.Handler(_context);
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

            var handler = new CancelFostering.Handler(_context);
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

            var handler = new CancelFostering.Handler(_context);
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

            var handler = new CancelFostering.Handler(_context);
            var result = await handler.Handle(new CancelFostering.Command
            {
                FosteringId = fostering.Id,
                UserId = userId
            }, default);

            Assert.True(result.IsSuccess);
        }
    }
}
