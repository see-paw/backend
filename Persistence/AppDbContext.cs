
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

/// <summary>
/// Represents the application's primary database context for Entity Framework Core.
/// </summary>
/// <remarks>
/// The <see cref="AppDbContext"/> class manages access to the database and provides
/// the configuration for the application's entities.  
/// It defines the <see cref="DbSet{TEntity}"/> properties used for querying and saving instances
/// of the domain models, such as <see cref="Animal"/>.  
/// 
/// The context also configures the mapping of enumeration properties to string values
/// to ensure data readability and consistency across database records.
/// </remarks>

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public required DbSet<Animal> Animals { get; set; }

    /// <summary>
    /// Configures the model schema and entity mappings for the database context.
    /// </summary>
    /// <param name="modelBuilder">
    /// The <see cref="ModelBuilder"/> instance used to configure entity relationships, properties, and conversions.
    /// </param>
    /// <remarks>
    /// This method is overridden to customize how Entity Framework Core maps the domain entities to the database schema.  
    /// In particular, it ensures that all enumeration properties in the <see cref="Animal"/> entity
    /// are stored as strings instead of their underlying integer values, improving database readability.
    /// </remarks>

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Configure enum properties to be stored as strings in the database instead of integers
        modelBuilder.Entity<Animal>(entity =>
        {
            entity.Property(a => a.AnimalState).HasConversion<string>();
            entity.Property(a => a.Species).HasConversion<string>();
            entity.Property(a => a.Size).HasConversion<string>();
            entity.Property(a => a.Sex).HasConversion<string>();
            entity.Property(a => a.Breed).HasConversion<string>();
        });
    }
}

