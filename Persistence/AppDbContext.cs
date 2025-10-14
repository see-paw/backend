
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public required DbSet<Animal> Animals { get; set; }

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

