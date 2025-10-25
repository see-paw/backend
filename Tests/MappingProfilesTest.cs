using AutoMapper;
using Domain;
using Domain.Enums;
using WebAPI.Core;
using WebAPI.DTOs;
using Xunit;

namespace Tests
{
    /// <summary>
    /// Unit tests for MappingProfiles.
    /// Validates AutoMapper configuration and mappings between entities and DTOs.
    /// </summary>
    public class MappingProfilesTest
    {
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configuration;

        public MappingProfilesTest()
        {
            _configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfiles>();
            });

            _mapper = _configuration.CreateMapper();
        }

        /// <summary>
        /// Tests that AutoMapper configuration is valid.
        /// </summary>
        [Fact]
        public void Configuration_IsValid()
        {
            // Act & Assert
            _configuration.AssertConfigurationIsValid();
        }

        /// <summary>
        /// Tests that Animal maps to ResAnimalDto correctly.
        /// </summary>
        [Fact]
        public void Map_AnimalToResAnimalDto_MapsAllProperties()
        {
            // Arrange
            var breedId = Guid.NewGuid().ToString();
            var shelterId = Guid.NewGuid().ToString();
            var birthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3));

            var animal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Max",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                AnimalState = AnimalState.Available,
                Colour = "Brown",
                BirthDate = birthDate,
                Sterilized = true,
                Cost = 100m,
                Description = "Friendly dog",
                Features = "Good with kids",
                ShelterId = shelterId,
                BreedId = breedId,
                Breed = new Breed
                {
                    Id = breedId,
                    Name = "Golden Retriever"
                },
                Images = new List<Image>()
            };

            // Act
            var dto = _mapper.Map<ResAnimalDto>(animal);

            // Assert
            Assert.Equal(animal.Id, dto.Id);
            Assert.Equal(animal.Name, dto.Name);
            Assert.Equal(animal.Species, dto.Species);
            Assert.Equal(animal.Size, dto.Size);
            Assert.Equal(animal.Sex, dto.Sex);
            Assert.Equal(animal.AnimalState, dto.AnimalState);
            Assert.Equal(animal.Colour, dto.Colour);
            Assert.Equal(animal.BirthDate, dto.BirthDate);
            Assert.Equal(animal.Sterilized, dto.Sterilized);
            Assert.Equal(animal.Cost, dto.Cost);
            Assert.Equal(animal.Description, dto.Description);
            Assert.Equal(animal.Features, dto.Features);
            Assert.Equal("Golden Retriever", dto.Breed.Name);
        }

        /// <summary>
        /// Tests that Age is calculated correctly from BirthDate.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        public void Map_AnimalToResAnimalDto_CalculatesAgeCorrectly(int yearsOld)
        {
            // Arrange
            var birthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-yearsOld));
            var animal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test Animal",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                AnimalState = AnimalState.Available,
                Colour = "Brown",
                BirthDate = birthDate,
                Sterilized = true,
                Cost = 100m,
                ShelterId = Guid.NewGuid().ToString(),
                BreedId = Guid.NewGuid().ToString(),
                Breed = new Breed { Id = Guid.NewGuid().ToString(), Name = "Test Breed" }
            };

            // Act
            var dto = _mapper.Map<ResAnimalDto>(animal);

            // Assert
            Assert.Equal(yearsOld, dto.Age);
        }

        /// <summary>
        /// Tests that BreedName is mapped from Breed.Name.
        /// </summary>
        [Fact]
        public void Map_AnimalToResAnimalDto_MapsBreedName()
        {
            // Arrange
            var animal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Buddy",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                AnimalState = AnimalState.Available,
                Colour = "White",
                BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
                Sterilized = true,
                Cost = 80m,
                ShelterId = Guid.NewGuid().ToString(),
                BreedId = Guid.NewGuid().ToString(),
                Breed = new Breed
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Persian"
                }
            };

            // Act
            var dto = _mapper.Map<ResAnimalDto>(animal);

            // Assert
            Assert.Equal("Persian", dto.Breed.Name);
        }

        /// <summary>
        /// Tests that Images collection is mapped correctly.
        /// </summary>
        [Fact]
        public void Map_AnimalToResAnimalDto_MapsImagesCollection()
        {
            // Arrange
            var animalId = Guid.NewGuid().ToString();
            var images = new List<Image>
            {
                new Image
                {
                    Id = Guid.NewGuid().ToString(),
                    Url = "https://example.com/image1.jpg",
                    IsPrincipal = true,
                    Description = "Main photo",
                    AnimalId = animalId
                },
                new Image
                {
                    Id = Guid.NewGuid().ToString(),
                    Url = "https://example.com/image2.jpg",
                    IsPrincipal = false,
                    Description = "Side view",
                    AnimalId = animalId
                }
            };

            var animal = new Animal
            {
                Id = animalId,
                Name = "Rocky",
                Species = Species.Dog,
                Size = SizeType.Large,
                Sex = SexType.Male,
                AnimalState = AnimalState.Available,
                Colour = "Gray",
                BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-4)),
                Sterilized = true,
                Cost = 120m,
                ShelterId = Guid.NewGuid().ToString(),
                BreedId = Guid.NewGuid().ToString(),
                Breed = new Breed { Id = Guid.NewGuid().ToString(), Name = "Husky" },
                Images = images
            };

            // Act
            var dto = _mapper.Map<ResAnimalDto>(animal);

            // Assert
            Assert.Equal(2, dto.Images.Count);
            Assert.Contains(dto.Images, img => img.IsPrincipal && img.Url == "https://example.com/image1.jpg");
            Assert.Contains(dto.Images, img => !img.IsPrincipal && img.Url == "https://example.com/image2.jpg");
        }

        /// <summary>
        /// Tests that Image maps to ResImageDto correctly.
        /// </summary>
        [Fact]
        public void Map_ImageToResImageDto_MapsAllProperties()
        {
            // Arrange
            var image = new Image
            {
                Id = Guid.NewGuid().ToString(),
                Url = "https://example.com/photo.jpg",
                IsPrincipal = true,
                Description = "Beautiful photo"
            };

            // Act
            var dto = _mapper.Map<ResImageDto>(image);

            // Assert
            Assert.Equal(image.Id, dto.Id);
            Assert.Equal(image.Url, dto.Url);
            Assert.Equal(image.IsPrincipal, dto.IsPrincipal);
            Assert.Equal(image.Description, dto.Description);
        }

        /// <summary>
        /// Tests that empty Images collection is handled correctly.
        /// </summary>
        [Fact]
        public void Map_AnimalWithNoImages_ReturnsEmptyImageCollection()
        {
            // Arrange
            var animal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Luna",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                AnimalState = AnimalState.Available,
                Colour = "Black",
                BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                Sterilized = false,
                Cost = 60m,
                ShelterId = Guid.NewGuid().ToString(),
                BreedId = Guid.NewGuid().ToString(),
                Breed = new Breed { Id = Guid.NewGuid().ToString(), Name = "Mixed" },
                Images = new List<Image>()
            };

            // Act
            var dto = _mapper.Map<ResAnimalDto>(animal);

            // Assert
            Assert.NotNull(dto.Images);
            Assert.Empty(dto.Images);
        }

        /// <summary>
        /// Tests mapping for different animal species.
        /// </summary>
        [Theory]
        [InlineData(Species.Dog)]
        [InlineData(Species.Cat)]
        public void Map_AnimalToResAnimalDto_MapsSpeciesCorrectly(Species species)
        {
            // Arrange
            var animal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Species = species,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                AnimalState = AnimalState.Available,
                Colour = "Brown",
                BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
                Sterilized = true,
                Cost = 100m,
                ShelterId = Guid.NewGuid().ToString(),
                BreedId = Guid.NewGuid().ToString(),
                Breed = new Breed { Id = Guid.NewGuid().ToString(), Name = "Test Breed" }
            };

            // Act
            var dto = _mapper.Map<ResAnimalDto>(animal);

            // Assert
            Assert.Equal(species, dto.Species);
        }

        /// <summary>
        /// Tests mapping for different animal states.
        /// </summary>
        [Theory]
        [InlineData(AnimalState.Available)]
        [InlineData(AnimalState.PartiallyFostered)]
        [InlineData(AnimalState.TotallyFostered)]
        [InlineData(AnimalState.HasOwner)]
        [InlineData(AnimalState.Inactive)]
        public void Map_AnimalToResAnimalDto_MapsAnimalStateCorrectly(AnimalState state)
        {
            // Arrange
            var animal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                AnimalState = state,
                Colour = "Brown",
                BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
                Sterilized = true,
                Cost = 100m,
                ShelterId = Guid.NewGuid().ToString(),
                BreedId = Guid.NewGuid().ToString(),
                Breed = new Breed { Id = Guid.NewGuid().ToString(), Name = "Test Breed" }
            };

            // Act
            var dto = _mapper.Map<ResAnimalDto>(animal);

            // Assert
            Assert.Equal(state, dto.AnimalState);
        }
    }
}