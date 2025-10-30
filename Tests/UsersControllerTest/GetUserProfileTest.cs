using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Application.Users.Queries;
using Domain;
using Persistence;
using Application.Core;


namespace Tests.UsersControllerTest
{
    //codacy: ignore[complexity]
    public class GetUserProfileTest
    {
        private readonly AppDbContext _context;
        private readonly GetUserProfile.Handler _handler;

        public GetUserProfileTest()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "GetUserProfileTestDB")
                .Options;

            _context = new AppDbContext(options);

           
            _context.Users.Add(new User
            {
                Id = "38bd42ca-c819-4496-be10-0d312a08c837",
                Name = "Diana Silva",
                BirthDate = new DateTime(1990, 9, 30),
                Street = "Rua das Oliveiras 10",
                City = "Faro",
                PostalCode = "8000-333",
                PhoneNumber = "912345678"
            });

            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            _handler = new GetUserProfile.Handler(_context);
        }

        [Fact]
        public async Task ShouldReturnUserProfile_WhenUserExists()
        {
            var query = new GetUserProfile.Query { UserId = "38bd42ca-c819-4496-be10-0d312a08c837" };

            var result = await _handler.Handle(query, CancellationToken.None);
          
            Assert.True(result.IsSuccess);
           
        }

        [Fact]
        public async Task ShouldFail_WhenUserNotFound()
        {
            var query = new GetUserProfile.Query { UserId = "38bd42ca-c819-4496-be10-0d312a08c838" };
            
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.False(result.IsSuccess);
           
        }
    }
}
