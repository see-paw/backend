using Application.Common;
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

            /// <summary>
            /// Shelter name used when creating a new AdminCAA account.
            /// </summary>
            public string? ShelterName { get; set; }
            
            /// <summary>
            /// Street of the shelter (AdminCAA only).
            /// </summary>
            public string? ShelterStreet { get; set; }
            
            /// <summary>
            /// City where the shelter is located (AdminCAA only).
            /// </summary>
            public string? ShelterCity { get; set; }
            
            /// <summary>
            /// Postal code of the shelter (AdminCAA only).
            /// </summary>
            public string? ShelterPostalCode { get; set; }
            
            /// <summary>
            /// Contact phone number of the shelter (AdminCAA only).
            /// </summary>
            public string? ShelterPhone { get; set; }
            
            /// <summary>
            /// Tax identification number (NIF) of the shelter (AdminCAA only).
            /// </summary>
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

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            /// <param name="dbContext">Database context used to persist shelters.</param>
            /// <param name="userManager">ASP.NET Identity user manager used to create users and assign roles.</param>
            public Handler(AppDbContext dbContext, UserManager<User> userManager)
            {
                _dbContext = dbContext;
                _userManager = userManager;
            }

            /// <summary>
            /// Executes the user registration process.
            /// Validates role, creates user, creates shelter if applicable, and assigns the proper role.
            /// </summary>
            /// <param name="request">Registration command containing user and optional shelter data.</param>
            /// <param name="ct">Cancellation token for async operation control.</param>
            /// <returns>A <see cref="Result{User}"/> indicating success or failure.</returns>
            public async Task<Result<User>> Handle(Command request, CancellationToken ct)
            {
                // Allowed roles to avoid privilege escalation
                var allowedRoles = new[] { AppRoles.User, AppRoles.AdminCAA };
                if (!allowedRoles.Contains(request.SelectedRole))
                    return Result<User>.Failure("Invalid role selected.", 400);

                // If AdminCAA → Create new Shelter before creating the User
                if (request.SelectedRole == AppRoles.AdminCAA)
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
