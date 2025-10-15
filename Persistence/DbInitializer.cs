

using Domain;
using Domain.Enums;

namespace Persistence;

/// <summary>
/// Provides database seeding functionality for the application.
/// </summary>
/// <remarks>
/// The <see cref="DbInitializer"/> class populates the database with initial data
/// when it is first created or found empty.  
/// It ensures that essential entities, such as <see cref="Animal"/>, are available
/// for testing and demonstration purposes.  
/// 
/// This seeding process is typically invoked during application startup 
/// (see <c>Program.cs</c>) after applying migrations.
/// </remarks>
public static class DbInitializer
{
    /// <summary>
    /// Seeds the database with initial <see cref="Animal"/> data if no records exist.
    /// </summary>
    /// <param name="dbContext">The application's database context used to access and modify data.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation of seeding the database.
    /// </returns>
    /// <remarks>
    /// This method checks whether the <see cref="AppDbContext.Animals"/> table already contains data.  
    /// If it is empty, it inserts a predefined list of animals with various attributes and states.  
    /// 
    /// The method is designed to be idempotent — running it multiple times will not duplicate data.  
    /// It is called during application startup, typically after migrations have been applied.
    /// </remarks>

    public static async Task SeedData(AppDbContext dbContext)
    {
        if (dbContext.Animals.Any())
        {
            return;
        }

        var animals = new List<Animal>
    {
        new Animal
        {
            Name = "Bolinhas",
            AnimalState = AnimalState.Available,
            Description = "Gato muito meigo e brincalhão, gosta de dormir ao sol.",
            Species = Species.Cat,
            Size = SizeType.Small,
            Sex = SexType.Male,
            Colour = "Branco e cinzento",
            BirthDate = new DateOnly(2022, 4, 15),
            Sterilized = true,
            Breed = Breed.Dobermann,
            Cost = 30,
            Features = "Olhos verdes, muito sociável",
            MainImageUrl = "https://exemplo.pt/imagens/bolinhas.jpg"
        },
        new Animal
        {
            Name = "Luna",
            AnimalState = AnimalState.Available,
            Description = "Cadela jovem e energética, ideal para famílias com crianças.",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Female,
            Colour = "Castanho claro",
            BirthDate = new DateOnly(2021, 11, 5),
            Sterilized = true,
            Breed = Breed.Other,
            Cost = 50,
            Features = "Muito obediente e adora correr",
            MainImageUrl = "https://exemplo.pt/imagens/luna.jpg"
        },
        new Animal
        {
            Name = "Tico",
            AnimalState = AnimalState.Available,
            Description = "Papagaio falador que adora companhia humana.",
            Species = Species.Cat,
            Size = SizeType.Small,
            Sex = SexType.Male,
            Colour = "Verde com azul",
            BirthDate = new DateOnly(2020, 2, 10),
            Sterilized = false,
            Breed = Breed.Other,
            Cost = 80,
            Features = "Sabe dizer 'Olá!' e assobiar",
            MainImageUrl = "https://exemplo.pt/imagens/tico.jpg"
        },
        new Animal
        {
            Name = "Mika",
            AnimalState = AnimalState.Available,
            Description = "Gata calma e dócil, procura um lar tranquilo.",
            Species = Species.Cat,
            Size = SizeType.Small,
            Sex = SexType.Female,
            Colour = "Preto",
            BirthDate = new DateOnly(2020, 8, 22),
            Sterilized = true,
            Breed = Breed.Other,
            Cost = 25,
            Features = "Olhos azuis intensos",
            MainImageUrl = "https://exemplo.pt/imagens/mika.jpg"
        },
        new Animal
        {
            Name = "Thor",
            AnimalState = AnimalState.Available,
            Description = "Cão de guarda muito protetor, mas fiel à família.",
            Species = Species.Dog,
            Size = SizeType.Large,
            Sex = SexType.Male,
            Colour = "Preto e castanho",
            BirthDate = new DateOnly(2019, 6, 30),
            Sterilized = false,
            Breed = Breed.Other,
            Cost = 100,
            Features = "Muito atento e obediente",
            MainImageUrl = "https://exemplo.pt/imagens/thor.jpg"
        },
        new Animal
        {
            Name = "Nina",
            AnimalState = AnimalState.Available,
            Description = "Coelha curiosa e afetuosa, gosta de cenouras e de brincar.",
            Species = Species.Dog,
            Size = SizeType.Small,
            Sex = SexType.Female,
            Colour = "Branco com manchas castanhas",
            BirthDate = new DateOnly(2023, 3, 10),
            Sterilized = false,
            Breed = Breed.Dobermann,
            Cost = 15,
            Features = "Orelhas pequenas e pelo macio",
            MainImageUrl = "https://exemplo.pt/imagens/nina.jpg"
        },
        new Animal
        {
            Name = "Rocky",
            AnimalState = AnimalState.Inactive,
            Description = "Cão atlético e leal, ideal para quem gosta de caminhadas.",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Cinza",
            BirthDate = new DateOnly(2022, 7, 19),
            Sterilized = true,
            Breed = Breed.Husky,
            Cost = 70,
            Features = "Olhos azuis e muita energia",
            MainImageUrl = "https://exemplo.pt/imagens/rocky.jpg"
        },
        new Animal
        {
            Name = "Amora",
            AnimalState = AnimalState.HasOwner,
            Description = "Gata jovem e curiosa, adora caçar brinquedos.",
            Species = Species.Cat,
            Size = SizeType.Small,
            Sex = SexType.Female,
            Colour = "Cinzento e branco",
            BirthDate = new DateOnly(2023, 5, 14),
            Sterilized = false,
            Breed = Breed.Other,
            Cost = 20,
            Features = "Bigodes longos e muito expressiva",
            MainImageUrl = "https://exemplo.pt/imagens/amora.jpg"
        },
        new Animal
        {
            Name = "Zeus",
            AnimalState = AnimalState.TotallyFostered,
            Description = "Cavalo calmo e bem treinado, ótimo para equitação.",
            Species = Species.Dog,
            Size = SizeType.Large,
            Sex = SexType.Male,
            Colour = "Castanho escuro",
            BirthDate = new DateOnly(2017, 9, 1),
            Sterilized = true,
            Breed = Breed.Husky,
            Cost = 500,
            Features = "Crina longa e brilhante",
            MainImageUrl = "https://exemplo.pt/imagens/zeus.jpg"
        },
        new Animal
        {
            Name = "Pipoca",
            AnimalState = AnimalState.PartiallyFostered,
            Description = "Hamster pequena e simpática, ideal para crianças.",
            Species = Species.Dog,
            Size = SizeType.Small,
            Sex = SexType.Female,
            Colour = "Dourado",
            BirthDate = new DateOnly(2024, 1, 12),
            Sterilized = false,
            Breed = Breed.PitBull,
            Cost = 10,
            Features = "Muito ativa e adora correr na roda",
            MainImageUrl = "https://exemplo.pt/imagens/pipoca.jpg"
        }
    };

        await dbContext.Animals.AddRangeAsync(animals);
        await dbContext.SaveChangesAsync();
    }

}

