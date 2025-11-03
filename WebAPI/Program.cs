using Application.Animals.Queries;
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
builder.Services.AddIdentityApiEndpoints<User>(opt =>
    {
        opt.User.RequireUniqueEmail = true;
    }).AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();
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
    .WithOrigins("http://localhost:3000", "https://localhost:3000"));

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