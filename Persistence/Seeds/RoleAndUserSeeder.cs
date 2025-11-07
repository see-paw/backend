using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Domain;
using Domain.Common;

namespace Persistence.Seeds;

internal static class RoleAndUserSeeder
{
    public static async Task SeedAsync(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILoggerFactory loggerFactory)
    {
        if (!userManager.Users.Any())
        {
            var logger = loggerFactory.CreateLogger("DbInitializer");
            
            foreach (var role in AppRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            
            var mainUsers = new List<(User user, string password, string role)>
            {
                (new User
                {
                    Id = SeedConstants.User2Id,
                    Name = "Bob Johnson",
                    UserName = "bob@test.com",
                    Email = "bob@test.com",
                    City = "Porto",
                    Street = "Rua das Flores 10",
                    PostalCode = "4000-123",
                    BirthDate = new DateTime(1995, 4, 12),
                    PhoneNumber = "912345678",
                    CreatedAt = DateTime.UtcNow
                }, SeedConstants.Password1, AppRoles.PlatformAdmin),
                
                (new User
                {
                    Id = SeedConstants.User1Id,
                    Name = "Alice Ferreira",
                    UserName = "alice@test.com",
                    Email = "alice@test.com",
                    City = "Lisboa",
                    Street = "Avenida da Liberdade 55",
                    PostalCode = "1250-123",
                    BirthDate = new DateTime(1998, 11, 2),
                    PhoneNumber = "934567890",
                    CreatedAt = DateTime.UtcNow,
                    ShelterId = SeedConstants.Shelter1Id
                }, SeedConstants.Password1, AppRoles.AdminCAA),
                
                (new User
                {
                    Id = SeedConstants.User3Id,
                    Name = "Carlos Santos",
                    UserName = "carlos@test.com",
                    Email = "carlos@test.com",
                    City = "Coimbra",
                    Street = "Rua do Penedo 32",
                    PostalCode = "3000-222",
                    BirthDate = new DateTime(1992, 6, 8),
                    PhoneNumber = "967123456",
                    CreatedAt = DateTime.UtcNow
                }, SeedConstants.Password1, AppRoles.User),
                
                (new User
                {
                    Id = SeedConstants.User4Id,
                    Name = "Diana Silva",
                    UserName = "diana@test.com",
                    Email = "diana@test.com",
                    City = "Faro",
                    Street = "Rua das Oliveiras 8",
                    PostalCode = "8000-333",
                    BirthDate = new DateTime(1990, 9, 30),
                    PhoneNumber = "925111333",
                    CreatedAt = DateTime.UtcNow
                }, SeedConstants.Password1, AppRoles.User),
                
                (new User
                {
                    Id = SeedConstants.User5Id,
                    Name = "Eduardo Lima",
                    UserName = "eduardo@test.com",
                    Email = "eduardo@test.com",
                    City = "Braga",
                    Street = "Rua Nova 42",
                    PostalCode = "4700-321",
                    BirthDate = new DateTime(1988, 2, 14),
                    PhoneNumber = "915222444",
                    CreatedAt = DateTime.UtcNow
                }, SeedConstants.Password1, AppRoles.User),
                
                (new User
                {
                    Id = SeedConstants.User6Id,
                    Name = "Filipe Marques",
                    UserName = "filipe@test.com",
                    Email = "filipe@test.com",
                    City = "Porto",
                    Street = "Rua das Oliveiras 99",
                    PostalCode = "4000-450",
                    BirthDate = new DateTime(1994, 5, 27),
                    PhoneNumber = "912345999",
                    CreatedAt = DateTime.UtcNow,
                    ShelterId = SeedConstants.Shelter2Id
                }, SeedConstants.Password1, AppRoles.AdminCAA),
                
                (new User
                {
                    Id = SeedConstants.User7Id,
                    Name = "Gustavo Pereira",
                    UserName = "gustavo@test.com",
                    Email = "gustavo@test.com",
                    City = "Lisboa",
                    Street = "Rua dos Favoritos 15",
                    PostalCode = "1200-100",
                    BirthDate = new DateTime(1993, 8, 20),
                    PhoneNumber = "918888777",
                    CreatedAt = DateTime.UtcNow
                }, SeedConstants.Password1, AppRoles.User),
                
                (new User
                {
                    Id = SeedConstants.User8Id,
                    Name = "Alice Notifications Admin",
                    UserName = "alice.notif@test.com",
                    Email = "alice.notif@test.com",
                    City = "Porto",
                    Street = "Rua dos Testes 100",
                    PostalCode = "4100-100",
                    BirthDate = new DateTime(1990, 1, 1),
                    PhoneNumber = "910000001",
                    CreatedAt = DateTime.UtcNow,
                    ShelterId = SeedConstants.Shelter3Id
                }, SeedConstants.Password1, AppRoles.AdminCAA),
                
                (new User
                {
                    Id = SeedConstants.User9Id,
                    Name = "Carlos Notifications User",
                    UserName = "carlos.notif@test.com",
                    Email = "carlos.notif@test.com",
                    City = "Porto",
                    Street = "Rua dos Testes 200",
                    PostalCode = "4100-200",
                    BirthDate = new DateTime(1992, 6, 8),
                    PhoneNumber = "910000002",
                    CreatedAt = DateTime.UtcNow
                }, SeedConstants.Password1, AppRoles.User),
                
                (new User
                {
                    Id = SeedConstants.OwnershipUser1Id,
                    UserName = "user1@test.com",
                    Email = "user1@test.com",
                    Name = "João Silva",
                    BirthDate = new DateTime(1990, 5, 15),
                    Street = "Rua das Flores, 123",
                    City = "Porto",
                    PostalCode = "4000-001",
                    PhoneNumber = "912345678",
                    EmailConfirmed = true
                }, SeedConstants.Password2, AppRoles.User),
                
                (new User
                {
                    Id = SeedConstants.OwnershipUser2Id,
                    UserName = "user2@test.com",
                    Email = "user2@test.com",
                    Name = "Maria Santos",
                    BirthDate = new DateTime(1985, 8, 20),
                    Street = "Avenida da Liberdade, 456",
                    City = "Lisboa",
                    PostalCode = "1250-001",
                    PhoneNumber = "923456789",
                    EmailConfirmed = true
                }, SeedConstants.Password2, AppRoles.User),
                
                (new User
                {
                    Id = SeedConstants.OwnershipUser3Id,
                    UserName = "user3@test.com",
                    Email = "user3@test.com",
                    Name = "Carlos Pereira",
                    BirthDate = new DateTime(1995, 3, 10),
                    Street = "Rua do Comércio, 789",
                    City = "Braga",
                    PostalCode = "4700-001",
                    PhoneNumber = "934567890",
                    EmailConfirmed = true
                }, SeedConstants.Password2, AppRoles.User),
                
                (new User
                {
                    Id = SeedConstants.FosterUserId,
                    UserName = "foster@test.com",
                    Email = "foster@test.com",
                    EmailConfirmed = true,
                    Name = "Foster Test User",
                    BirthDate = new DateTime(1990, 1, 1),
                    Street = "Rua dos Fosters 123",
                    City = "Porto",
                    PostalCode = "4000-001",
                    PhoneNumber = "912345678",
                    CreatedAt = DateTime.UtcNow
                }, SeedConstants.Password1, AppRoles.User),
                
                (new User
                {
                    Id = SeedConstants.RegularUserId,
                    UserName = "regular@test.com",
                    Email = "regular@test.com",
                    EmailConfirmed = true,
                    Name = "Regular Test User",
                    BirthDate = new DateTime(1992, 5, 15),
                    Street = "Rua Regular 456",
                    City = "Lisboa",
                    PostalCode = "1000-001",
                    PhoneNumber = "913456789",
                    CreatedAt = DateTime.UtcNow
                }, SeedConstants.Password1, AppRoles.User),
                
                (new User
                {
                    Id = SeedConstants.CancelFosterUserId,
                    UserName = "cancel-foster@test.com",
                    Email = "cancel-foster@test.com",
                    EmailConfirmed = true,
                    Name = "Cancel Foster User",
                    BirthDate = new DateTime(1990, 1, 1),
                    Street = "Rua Cancel 123",
                    City = "Porto",
                    PostalCode = "4000-001",
                    PhoneNumber = "912345678",
                    CreatedAt = DateTime.UtcNow
                }, SeedConstants.Password1, AppRoles.User),
                
                (new User
                {
                    Id = SeedConstants.OtherCancelUserId,
                    UserName = "other-cancel@test.com",
                    Email = "other-cancel@test.com",
                    EmailConfirmed = true,
                    Name = "Other Cancel User",
                    BirthDate = new DateTime(1992, 5, 15),
                    Street = "Rua Other 456",
                    City = "Lisboa",
                    PostalCode = "1000-001",
                    PhoneNumber = "913456789",
                    CreatedAt = DateTime.UtcNow
                }, SeedConstants.Password1, AppRoles.User)
            };

            foreach (var (user, password, roleAssignment) in mainUsers)
            {
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, roleAssignment);
                }
                else
                {
                    logger.LogWarning("Erro ao criar utilizador {Email}: {Errors}", user.Email,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}