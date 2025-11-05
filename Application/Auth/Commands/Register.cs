using Application.Core;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence;

namespace Application.Auth.Commands;

public class Register
{
    public class Command : IRequest<Result<User>>
    {
        public User User { get; set; }
        public string Password { get; set; }
        //if the frotend send selected role = null, it's safer to default to "User" because there's no permission to manage animals
        public string SelectedRole { get; set; } = "User"; // Default safest role

        // Shelter fields (only used if SelectedRole == "AdminCAA")
        public string? ShelterName { get; set; }
        public string? ShelterStreet { get; set; }
        public string? ShelterCity { get; set; }
        public string? ShelterPostalCode { get; set; }
        public string? ShelterPhone { get; set; }
        public string? ShelterNIF { get; set; }
        public String? ShelterOpeningTime { get; set; }
        public String? ShelterClosingTime { get; set; }
    }

    public class Handler(AppDbContext dbContext, UserManager<User> userManager)
        : IRequestHandler<Command, Result<User>>
    {
        public async Task<Result<User>> Handle(Command request, CancellationToken ct)
        {
            // Allowed roles
            var allowedRoles = new[] { "User", "AdminCAA" };
            if (!allowedRoles.Contains(request.SelectedRole))
                return Result<User>.Failure("Invalid role selected.", 400);

            // If Account is AdminCAA, Create new Shelter
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

                dbContext.Shelters.Add(shelter);
                await dbContext.SaveChangesAsync(ct);

                request.User.ShelterId = shelter.Id;
                request.User.Shelter = shelter;
            }

            // Create user with password hashing
            var createResult = await userManager.CreateAsync(request.User, request.Password);
            if (!createResult.Succeeded)
                return Result<User>.Failure(createResult.Errors.First().Description, 400);

            // Add role
            var roleResult = await userManager.AddToRoleAsync(request.User, request.SelectedRole);
            if (!roleResult.Succeeded)
                return Result<User>.Failure(roleResult.Errors.First().Description, 400);


            return Result<User>.Success(request.User, 201);

           
        }
    }
}
