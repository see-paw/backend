using Application.Core;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Auth.Commands;

public class Register
{
    public class Command : IRequest<Result>
    {
        public User User { get; set; }
        public string Password { get; set; }
        //if the frotend send selected role = null, it's safer to default to "User" because there's no permission to manage animals
        public string SelectedRole { get; set; } = "User"; // Default safest role
    }

    public class Handler(UserManager<User> userManager)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken ct)
        {
            // Allowed roles
            var allowedRoles = new[] { "User", "AdminCAA" };
            if (!allowedRoles.Contains(request.SelectedRole))
                return Result.Failure("Invalid role selected.", 400);

            // If Account is AdminCAA, Create new Shelter
            if (request.SelectedRole == "AdminCAA")
            {
                var shelter = new Shelter
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.User.Name, 
                    Street = request.User.Street,
                    City = request.User.City,
                    PostalCode = request.User.PostalCode,
                    PhoneNumber = request.User.PhoneNumber
                };

                context.Shelters.Add(shelter);
                await context.SaveChangesAsync(ct);

                request.User.ShelterId = shelter.Id;
            }

            // Create user with password hashing
            var createResult = await userManager.CreateAsync(request.User, request.Password);
            if (!createResult.Succeeded)
                return Result.Failure(createResult.Errors.First().Description, 400);

            // Add role
            await userManager.AddToRoleAsync(request.User, request.SelectedRole);

            // Generate token because after registration user is logged in
            var token = await tokenService.CreateToken(request.User);

            return Result<string>.Success(token, 201);
        }
    }
}
