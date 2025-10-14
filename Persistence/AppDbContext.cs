using Domain;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace Persistence;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public required DbSet<Animal> Animals { get; set; }
    public required DbSet<Shelter> Shelters { get; set; }

    public required DbSet<Image> Images { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //to test the endointity relationship between Shelter and Animal while there's no autentication implemented- delete afterwards
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
        }
    );
        // Configure the one-to-many relationship between Shelter and Animal with restricted delete behavior
        modelBuilder.Entity<Animal>()
       .HasOne(a => a.Shelter)//ver se quando tiver a tabela Shelter criada deixa de dar erro
       .WithMany(s => s.Animals)
       .HasForeignKey(a => a.ShelterId)
       .OnDelete(DeleteBehavior.Restrict);// Prevent cascade delete: when a Shelter is deleted, its Animals are not deleted

        // Configure the one-to-many relationship between Animal and Image with restricted delete behavior
        modelBuilder.Entity<Image>()
            .HasOne(i => i.Animal)
            .WithMany(a => a.Images)
            .HasForeignKey(i => i.AnimalId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete: deleting an Animal won't delete its Images

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

