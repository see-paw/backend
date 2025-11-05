using Application.Auth.Commands;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace Tests.AuthTests.Handlers
{
    //codacy: ignore[complexity]
    public class RegisterUserHandlerTests
    {
        private readonly AppDbContext _dbContext;
        private readonly Register.Handler _handler;
        private readonly UserManager<User> _userManager;

        public RegisterUserHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);

            // Mock UserManager
            _userManager = CreateMockedUserManager();

            _handler = new Register.Handler(_dbContext, _userManager);
        }

        private User CreateUser()
        {
            return new User()
            {

                Name = "Test User",
                Email = "test@example.com",
                Street = "Rua de Teste",
                City = "Porto",
                PostalCode = "4000-123",
                BirthDate = new DateTime(1990, 1, 1)
            };
        }

        // Successfully registers a normal user
        [Fact]
        public async Task RegisterNormalUser_Success()
        {
            var user = CreateUser();
            var command = new Register.Command
            {
                User = user,
                Password = "Aa!123456",
                SelectedRole = "User"
            };
            var result = await _handler.Handle(command, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        // Successfully registers an AdminCAA user with shelter creation
        [Fact]
        public async Task RegisterAdminCAAUser_Success()
        {
            var user = CreateUser();
            var command = new Register.Command
            {
                User = user,
                Password = "Aa!12345",
                SelectedRole = "AdminCAA",
                ShelterName = "Test Shelter",
                ShelterStreet = "Shelter Street",
                ShelterCity = "Shelter City",
                ShelterPostalCode = "1234-567",
                ShelterPhone = "123456789",
                ShelterNIF = "987654321",
                ShelterOpeningTime = "09:00",
                ShelterClosingTime = "18:00"
                };
            var result = await _handler.Handle(command, CancellationToken.None);
            Assert.True(result.IsSuccess);

            var createdShelter = await _dbContext.Shelters.FirstOrDefaultAsync();
            Assert.Equal("Test Shelter", createdShelter!.Name);
        }

        // Fails to register with an invalid role
        [Fact]
        public async Task InvalidRoleShouldFail()
        {
            var user = CreateUser();
            var command = new Register.Command
            {
                User = user,
                Password = "Aa!123456",
                SelectedRole = "InvalidRole"
            };
            var result = await _handler.Handle(command, CancellationToken.None);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task ShouldFail_WhenUserManagerCreateFails()
        {
            var user = CreateUser();

            var userManager = CreateMockedUserManagerFailureOnCreate();
            var handler = new Register.Handler(_dbContext, userManager);

            var command = new Register.Command
            {
                User = user,
                Password = "Aa!123456",
                SelectedRole = "User"
            };

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task ShouldFail_WhenAddToRoleFails()
        {
            var user = CreateUser();
            var userManager = CreateMockedUserManagerFailureOnRoleAssign();
            var handler = new Register.Handler(_dbContext, userManager);

            var command = new Register.Command
            {
                User = user,
                Password = "Aa!123456",
                SelectedRole = "User"
            };

            var result = await handler.Handle(command, CancellationToken.None);
            Assert.False(result.IsSuccess);
        }

        private static UserManager<User> CreateMockedUserManagerFailureOnRoleAssign()
        {
            var store = new Mock<IUserStore<User>>();
            var userManager = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);

            userManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            userManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to assign role" }));

            return userManager.Object;
        }

        private static UserManager<User> CreateMockedUserManagerFailureOnCreate()
        {
            var store = new Mock<IUserStore<User>>();
            var userManager = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);

            userManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed" }));

            return userManager.Object;
        }

        private static UserManager<User> CreateMockedUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            var userManager = new Mock<UserManager<User>>(
                store.Object,
                null, null, null, null, null, null, null, null
            );

            userManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            userManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            return userManager.Object;
        }

    }
}
