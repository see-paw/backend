using Domain;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace Persistence
{
    /// <summary>
    /// Represents the application's database context, responsible for managing
    /// entity configurations, relationships, and database interactions.
    /// </summary>
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Shelter> Shelters { get; set; }
        public DbSet<Photo> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed test shelters (temporary data for development while authentication is not implemented)
            modelBuilder.Entity<Shelter>().HasData(
                new Shelter
                {
                    ShelterId = "11111111-1111-1111-1111-111111111111",
                    Name = "Test Shelter",
                    Street = "Rua das Flores 123",
                    City = "Porto",
                    PostalCode = "4000-123",
                    Phone = "912345678",
                    NIF = "123456789",
                    OpeningTime = new TimeSpan(9, 0, 0),
                    ClosingTime = new TimeSpan(18, 0, 0),
                    CreatedAt = DateTime.UtcNow
                },
                new Shelter
                {
                    ShelterId = "22222222-2222-2222-2222-222222222222",
                    Name = "Test Shelter 2",
                    Street = "Rua de cima 898",
                    City = "Porto",
                    PostalCode = "4000-125",
                    Phone = "224589631",
                    NIF = "999999999",
                    OpeningTime = new TimeSpan(9, 0, 0),
                    ClosingTime = new TimeSpan(18, 0, 0),
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Configure the one-to-many relationship between Shelter and Animal (restricted delete)
            modelBuilder.Entity<Animal>()
                .HasOne(a => a.Shelter)
                .WithMany(s => s.Animals)
                .HasForeignKey(a => a.ShelterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the one-to-many relationship between Animal and Photo (restricted delete)
            modelBuilder.Entity<Photo>()
                .HasOne(i => i.Animal)
                .WithMany(a => a.Images)
                .HasForeignKey(i => i.AnimalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Store enum properties as strings in the database for readability
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
}
