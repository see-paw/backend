using Application.Animals.Queries;
using WebAPI.Validators;
using FluentValidation;
using Persistence;
using System.Text.Json.Serialization;
using Application;
using Application.Animals;
using Application.Core;
using Application.Fosterings;
using Application.Interfaces;
using Application.Services;
using Domain;
using Domain.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Hubs;
using Infrastructure.Images;
using Infrastructure.Notifications;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Text.Json.Serialization;
using WebAPI.Core;
using WebAPI.Middleware;
using WebAPI.Validators.Animals;

var inContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"; // Azure provides this env variable

string environmentName;
if (isProduction)
{
    environmentName = "Production";
}
else if (inContainer)
{
    environmentName = "Docker";
}
else
{
    environmentName = "Development";
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = environmentName
});

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Docker"))
    builder.Configuration.AddUserSecrets<Program>(optional: true);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection") ??
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string not found.");

// Services
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddCors();
builder.Services.AddMediatR(x => {
    x.RegisterServicesFromAssemblyContaining<GetAnimalDetails.Handler>();
    x.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddScoped<ISlotNormalizer, SlotNormalizer>();
builder.Services.AddScoped<IScheduleAssembler, ScheduleAssembler>();
builder.Services.AddScoped<ITimeRangeCalculator, TimeRangeCalculator>();

builder.Services.AddScoped<IFosteringService, FosteringService>();
builder.Services.AddScoped<FosteringDomainService>();
builder.Services.AddScoped(typeof(IImagesUploader<>), typeof(ImagesUploader<>));
builder.Services.AddScoped(typeof(IImageOwnerLoader<>), typeof(ImageOwnerLoader<>));
builder.Services.AddScoped<IPrincipalImageEnforcer, PrincipalImageEnforcer>();
builder.Services.AddScoped(typeof(IImageManager<>), typeof(ImageManager<>));
builder.Services.AddScoped<IImageOwnerLinker<Animal>, AnimalImageLinker>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IUserAccessor, UserAccessor>();
builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<GetAnimalDetailsValidator>();
builder.Services.AddTransient<ExceptionMiddleware>();

// This registers ASP.NET Core Identity using *API endpoints* instead of MVC UI.
// It automatically provides:
//   POST /api/register
//   POST /api/login
//   POST /api/logout
//
// It also configures token-based authentication (Bearer tokens), NOT cookies.
builder.Services.AddIdentityApiEndpoints<User>(opt =>
{
    // Ensures no two accounts share the same email
    opt.User.RequireUniqueEmail = true;
    // Enables support for roles (e.g., "User", "AdminCAA")
}).AddRoles<IdentityRole>()
    // Tells Identity to store users and roles in the AppDbContext
    .AddEntityFrameworkStores<AppDbContext>();

//Tell ASP.NET Core that the default auth mechanism is Bearer tokens (not cookies).
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.BearerScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
    options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
});
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthMiddlewareHandler>();

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<FosteringSettings>(
    builder.Configuration.GetSection("Fostering")
);

// Notification Services
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationService>();


var app = builder.Build();

// Pipeline
app.UseCors(c => c
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:8080")); // 8080 for tests with Python HTTP server (SignalR test)

app.UseMiddleware<IdentityResponseMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");
app.MapGroup("api").MapIdentityApi<User>();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Program>();

    logger.LogInformation("=================================================");
    logger.LogInformation($"Environment: {app.Environment.EnvironmentName}");
    logger.LogInformation($"Is Production: {app.Environment.IsProduction()}");
    logger.LogInformation("=================================================");

    // Apply migrations
    await context.Database.MigrateAsync();
    logger.LogInformation("Migrations applied successfully.");

    // Ensure Roles Exist
    var roles = new[] { "User", "AdminCAA" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            logger.LogInformation($"Role '{role}' created.");
        }
    }

    // How the DB is seeded dependending on the environment type
    if (app.Environment.IsProduction())
    {
        // Production mode (currently for testing): will only seed if the DB is empty
        if (!context.Shelters.Any())
        {
            logger.LogWarning("PRODUCTION MODE - Database is empty. Running seed for the FIRST TIME.");
            await DbInitializer.SeedData(context, userManager, roleManager, loggerFactory, false);
            logger.LogInformation("Database seeded successfully.");
        }
        else
        {
            logger.LogInformation("PRODUCTION MODE - Database already has data. Skipping seed.");
        }
    }
    else
    {
        logger.LogWarning($"DEVELOPMENT MODE ({app.Environment.EnvironmentName}) - Database will be reset and seeded.");
        await DbInitializer.SeedData(context, userManager, roleManager, loggerFactory, true);
        logger.LogInformation("Database seeded successfully.");
    }
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration or seeding.");
}

app.Run();