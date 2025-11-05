using Application.Core;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence;

namespace Application.Auth.Commands
{
    /// <summary>
    /// Handles registration of new user accounts, supporting both standard users and AdminCAA users.
    /// If the role is AdminCAA, a Shelter entity is created and linked to the user.
    /// </summary>
    public class Register
    {
        /// <summary>
        /// Command request used to register a new account.
        /// </summary>
        public class Command : IRequest<Result<User>>
        {
            /// <summary>
            /// Core user entity to be created.
            /// Must contain basic identity fields such as name, birthdate, etc.
            /// </summary>
            public User User { get; set; }

            /// <summary>
            /// Raw password that will be hashed before persisting.
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// Requested role for the newly created account.
            /// Defaults to "User" when not supplied.
            /// Allowed roles: "User", "AdminCAA".
            /// </summary>
            public string SelectedRole { get; set; } = "User";

            // ----- SHELTER FIELDS (Only required when SelectedRole == "AdminCAA") -----

            public string? ShelterName { get; set; }
            public string? ShelterStreet { get; set; }
            public string? ShelterCity { get; set; }
            public string? ShelterPostalCode { get; set; }
            public string? ShelterPhone { get; set; }
            public string? ShelterNIF { get; set; }

            /// <summary>
            /// Shelter opening time (e.g., "09:00"), parsed into TimeOnly on creation.
            /// </summary>
            public string? ShelterOpeningTime { get; set; }

            /// <summary>
            /// Shelter closing time (e.g., "18:00"), parsed into TimeOnly on creation.
            /// </summary>
            public string? ShelterClosingTime { get; set; }
        }

        /// <summary>
        /// Handles execution of the registration command.
        /// Creates the user, optionally creates a shelter if AdminCAA is selected, and assigns the role.
        /// </summary>
        public class Handler : IRequestHandler<Command, Result<User>>
        {
            private readonly AppDbContext _dbContext;
            private readonly UserManager<User> _userManager;

            public Handler(AppDbContext dbContext, UserManager<User> userManager)
            {
                _dbContext = dbContext;
                _userManager = userManager;
            }

            public async Task<Result<User>> Handle(Command request, CancellationToken ct)
            {
                // Allowed roles to avoid privilege escalation
                var allowedRoles = new[] { "User", "AdminCAA" };
                if (!allowedRoles.Contains(request.SelectedRole))
                    return Result<User>.Failure("Invalid role selected.", 400);

                // If AdminCAA → Create new Shelter before creating the User
                if (request.SelectedRole == "AdminCAA")
                {
                    var shelter = new Shelter
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = request.ShelterName!,
                        Street = request.ShelterStreet!,
                        City = request.ShelterCity!,
                        PostalCode = request.ShelterPostalCode!,
                        Phone = request.ShelterPhone!,
                        NIF = request.ShelterNIF!,
                        OpeningTime = TimeOnly.Parse(request.ShelterOpeningTime!),
                        ClosingTime = TimeOnly.Parse(request.ShelterClosingTime!)
                    };

                    _dbContext.Shelters.Add(shelter);
                    await _dbContext.SaveChangesAsync(ct);

                    // Link the user to the shelter
                    request.User.ShelterId = shelter.Id;
                    request.User.Shelter = shelter;
                }

                // Create user with hashed password
                var createResult = await _userManager.CreateAsync(request.User, request.Password);
                if (!createResult.Succeeded)
                    return Result<User>.Failure(createResult.Errors.First().Description, 400);

                // Assign role to user
                var roleResult = await _userManager.AddToRoleAsync(request.User, request.SelectedRole);
                if (!roleResult.Succeeded)
                    return Result<User>.Failure(roleResult.Errors.First().Description, 400);

                return Result<User>.Success(request.User, 201);
            }
        }
    }
}
