using API.DTOs;
using API.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Text.Json.Serialization;

var inContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // If running inside a container, set the environment to "Docker"
    EnvironmentName = inContainer ? "Docker" : null
});

//Load configuration from multiple sources:
// - appsettings.json (base)
// - appsettings.{Environment}.json (environment-specific)
// - Environment variables
// - User secrets (only in Development, not in Docker)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Docker"))
    builder.Configuration.AddUserSecrets<Program>(optional: true);

//Retrieve connection string either from configuration or environment variable
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection") ??
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");


if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string not found.");

// Register Entity Framework Core with PostgreSQL provider
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// AutoMapper is used to map DTOs<-> Entities across layers
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configure JSON serialization options:
// - Serialize enums as strings (not integers)
// - Make property names case-insensitive (e.g., "Name" or "name" both work)
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// Automatically trigger FluentValidation for DTOs during model binding and register all validators found in the same assembly as BaseAnimalDTO
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<BaseAnimalDTO>();

// Allows requests from the frontend 
builder.Services.AddCors();

// Custom middleware to handle exceptions globally
builder.Services.AddTransient<ExceptionMiddleware>();

//build the application
var app = builder.Build();

// This middleware intercepts all unhandled exceptions in controllers,
// formats them as JSON (using AppException), and returns consistent HTTP errors
app.UseMiddleware<ExceptionMiddleware>();

// Allow specific frontend origins to access the API
app.UseCors(c => c
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins("http://localhost:3000", "https://localhost:3000"));

// Map all controller routes
app.MapControllers();

// On application startup, apply any pending EF Core migrations.
// This ensures the database schema is up to date automatically.
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration.");
}

// Begin listening for incoming HTTP requests
app.Run();