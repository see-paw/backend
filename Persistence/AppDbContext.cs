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
/// of the domain models.  
/// 
/// The context also configures the mapping of enumeration properties to string values
/// to ensure data readability and consistency across database records.
/// </remarks>

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Animal> Animals { get; set; }
    public DbSet<Shelter> Shelters { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Fostering> Fosterings { get; set; }
    public DbSet<OwnershipRequest> OwnershipRequests { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Image> Images { get; set; }

    public DbSet<Breed> Breeds { get; set; }

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

        // ========== SHELTER CONFIGURATIONS ==========

        modelBuilder.Entity<Shelter>(entity =>
        {
            // NIF único
            entity.HasIndex(s => s.NIF).IsUnique();
        });

        // Shelter - Animal (1:N)
        modelBuilder.Entity<Animal>()
            .HasOne(animal => animal.Shelter)
            .WithMany(shelter => shelter.Animals)
            .HasForeignKey(animal => animal.ShelterId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== USER CONFIGURATIONS ==========

        modelBuilder.Entity<User>(entity =>
        {
            // Email is unique
            entity.HasIndex(user => user.Email).IsUnique();
        });

        // User - Shelter (N:1, only one Admin CAA)
        modelBuilder.Entity<User>()
            .HasOne(user => user.Shelter)
            .WithOne()
            .HasForeignKey<User>(user => user.ShelterId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // User - Animal (Owner) (1:N, optional)
        modelBuilder.Entity<Animal>()
            .HasOne(animal => animal.Owner) // only one ownership per animal
            .WithMany(user => user.OwnedAnimals) // an user can own multiple animals
            .HasForeignKey(animal => animal.OwnerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
        .HasIndex(u => u.ShelterId)
        .IsUnique() // Only one Admin CAA per shelter
        .HasFilter("\"ShelterId\" IS NOT NULL");  // Only applied when ShelterId is not null

        // ========== ANIMAL CONFIGURATIONS ==========

        modelBuilder.Entity<Animal>(entity =>
        {
            // Enum conversions
            entity.Property(a => a.AnimalState).HasConversion<string>();
            entity.Property(a => a.Species).HasConversion<string>();
            entity.Property(a => a.Size).HasConversion<string>();
            entity.Property(a => a.Sex).HasConversion<string>();
        });

        // ========== BREED CONFIGURATIONS ==========

        modelBuilder.Entity<Breed>(entity =>
        {
            // Breed name único
            entity.HasIndex(b => b.Name).IsUnique();
        });

        // Animal ↔ Breed (N:1)
        modelBuilder.Entity<Animal>()
            .HasOne(a => a.Breed)
            .WithMany(b => b.Animals)
            .HasForeignKey(a => a.BreedId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== IMAGE CONFIGURATIONS ==========

        modelBuilder.Entity<Image>(entity =>
        {
            // Only one main image per animal
            entity.HasIndex(i => new { i.AnimalId, i.IsPrincipal })
                  .IsUnique()
                  .HasFilter("\"IsPrincipal\" = true");
        });

        // Animal - Image (1:N)
        modelBuilder.Entity<Image>()
            .HasOne(i => i.Animal)
            .WithMany(a => a.Images)
            .HasForeignKey(i => i.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);  // If animal is deleted, deletes its images

        modelBuilder.Entity<Image>()
            .HasOne(i => i.Shelter)
            .WithMany(s => s.Images)
            .HasForeignKey(i => i.ShelterId)
            .OnDelete(DeleteBehavior.Cascade);

        // ========== FOSTERING CONFIGURATIONS ==========

        modelBuilder.Entity<Fostering>(entity =>
        {
            entity.Property(f => f.Status).HasConversion<string>();
        });

        modelBuilder.Entity<Fostering>()
            .HasOne(f => f.Animal)
            .WithMany(a => a.Fosterings)
            .HasForeignKey(f => f.AnimalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Fostering>()
            .HasOne(f => f.User)
            .WithMany(u => u.Fosterings)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== OWNERSHIP REQUEST CONFIGURATIONS ==========

        modelBuilder.Entity<OwnershipRequest>(entity =>
        {
            entity.Property(or => or.Status).HasConversion<string>();

            // An user can only have one ownership request per animal
            entity.HasIndex(or => new { or.AnimalId, or.UserId })
                  .IsUnique();
        });

        modelBuilder.Entity<OwnershipRequest>()
            .HasOne(or => or.Animal)
            .WithMany(a => a.OwnershipRequests)
            .HasForeignKey(or => or.AnimalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OwnershipRequest>()
            .HasOne(or => or.User)
            .WithMany(u => u.OwnershipRequests)
            .HasForeignKey(or => or.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== ACTIVITY CONFIGURATIONS ==========

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.Property(a => a.Type).HasConversion<string>();
            entity.Property(a => a.Status).HasConversion<string>();

            // An animal can only have one activity starting at a specific date
            entity.HasIndex(a => new { a.AnimalId, a.StartDate })
                  .IsUnique();
        });

        modelBuilder.Entity<Activity>()
            .HasOne(a => a.Animal)
            .WithMany(an => an.Activities)
            .HasForeignKey(a => a.AnimalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Activity>()
            .HasOne(a => a.User)
            .WithMany(u => u.Activities)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== FAVORITE CONFIGURATIONS ==========

        modelBuilder.Entity<Favorite>(entity =>
        {
            // An user cannot have the same animal as favorite more than once
            entity.HasIndex(f => new { f.UserId, f.AnimalId })
                  .IsUnique();
        });

        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Animal)
            .WithMany(a => a.Favorites)
            .HasForeignKey(f => f.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);  // If animal is deleted, deletes favorites for that animal

        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);  // If user is deleted, deletes user's favorite animals
    }
}

