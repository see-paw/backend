using AutoMapper;
using Domain;
using Domain.Enums;
using WebAPI.Core;
using WebAPI.DTOs;
using WebAPI.DTOs.Animals;
using WebAPI.DTOs.Images;
using WebAPI.DTOs.Ownership;
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
        /// Tests that BreedName is mapped from Breed.ShelterName.
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
            var publicId =  Guid.NewGuid().ToString();
            var images = new List<Image>
            {
                new Image
                {
                    Id = Guid.NewGuid().ToString(),
                    Url = "https://example.com/image1.jpg",
                    PublicId = publicId,
                    IsPrincipal = true,
                    Description = "Main photo",
                    AnimalId = animalId
                },
                new Image
                {
                    Id = Guid.NewGuid().ToString(),
                    Url = "https://example.com/image2.jpg",
                    PublicId = publicId,
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
            var publicId =  Guid.NewGuid().ToString();
            // Arrange
            var image = new Image
            {
                Id = Guid.NewGuid().ToString(),
                Url = "https://example.com/photo.jpg",
                PublicId = publicId,
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
        
         #region OwnershipRequest to ResUserOwnershipsDto Tests

    [Fact]
    public void Map_OwnershipRequest_ToDto_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var ownershipRequest = new OwnershipRequest
        {
            Id = "request123",
            AnimalId = "animal456",
            UserId = "user789",
            Amount = 150.50m,
            Status = OwnershipStatus.Pending,
            RequestInfo = "I love dogs",
            RequestedAt = new DateTime(2024, 10, 15, 10, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 10, 16, 14, 20, 0, DateTimeKind.Utc),
            Animal = new Animal
            {
                Id = "animal456",
                Name = "Max",
                AnimalState = AnimalState.Available,
                Cost = 150.50m,
                Images = new List<Image>
                {
                    new Image { Id = "img1",PublicId = "1", IsPrincipal = true, Url = "url1.jpg" },
                    new Image { Id = "img2",PublicId = "1", IsPrincipal = false, Url = "url2.jpg" }
                }
            },
            User = new User
            {
                Id = "user789",
                Name = "John Doe"
            }
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(ownershipRequest);

        // Assert
        Assert.Equal("request123", result.Id);
        Assert.Equal("animal456", result.AnimalId);
        Assert.Equal("Max", result.AnimalName);
        Assert.Equal(AnimalState.Available, result.AnimalState);
        Assert.NotNull(result.Image);
        Assert.Equal("img1", result.Image.Id);
        Assert.True(result.Image.IsPrincipal);
        Assert.Equal(150.50m, result.Amount);
        Assert.Equal(OwnershipStatus.Pending, result.OwnershipStatus);
        Assert.Equal("I love dogs", result.RequestInfo);
        Assert.Equal(new DateTime(2024, 10, 15, 10, 30, 0, DateTimeKind.Utc), result.RequestedAt);
        Assert.Equal(new DateTime(2024, 10, 16, 14, 20, 0, DateTimeKind.Utc), result.UpdatedAt);
        Assert.Null(result.ApprovedAt); // Should not be mapped for OwnershipRequest
    }

    [Fact]
    public void Map_OwnershipRequest_WithPendingStatus_MapsCorrectly()
    {
        // Arrange
        var ownershipRequest = new OwnershipRequest
        {
            Status = OwnershipStatus.Pending,
            Animal = new Animal { Images = new List<Image>() },
            User = new User()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(ownershipRequest);

        // Assert
        Assert.Equal(OwnershipStatus.Pending, result.OwnershipStatus);
    }

    [Fact]
    public void Map_OwnershipRequest_WithAnalysingStatus_MapsCorrectly()
    {
        // Arrange
        var ownershipRequest = new OwnershipRequest
        {
            Status = OwnershipStatus.Analysing,
            Animal = new Animal { Images = new List<Image>() },
            User = new User()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(ownershipRequest);

        // Assert
        Assert.Equal(OwnershipStatus.Analysing, result.OwnershipStatus);
    }

    [Fact]
    public void Map_OwnershipRequest_WithRejectedStatus_MapsCorrectly()
    {
        // Arrange
        var ownershipRequest = new OwnershipRequest
        {
            Status = OwnershipStatus.Rejected,
            UpdatedAt = DateTime.UtcNow.AddDays(-5),
            Animal = new Animal { Images = new List<Image>() },
            User = new User()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(ownershipRequest);

        // Assert
        Assert.Equal(OwnershipStatus.Rejected, result.OwnershipStatus);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public void Map_OwnershipRequest_WithApprovedStatus_MapsCorrectly()
    {
        // Arrange
        var ownershipRequest = new OwnershipRequest
        {
            Status = OwnershipStatus.Approved,
            Animal = new Animal { Images = new List<Image>() },
            User = new User()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(ownershipRequest);

        // Assert
        Assert.Equal(OwnershipStatus.Approved, result.OwnershipStatus);
    }

    [Fact]
    public void Map_OwnershipRequest_WithNoPrincipalImage_ReturnsNull()
    {
        // Arrange
        var ownershipRequest = new OwnershipRequest
        {
            Animal = new Animal
            {
                Images = new List<Image>
                {
                    new Image { Id = "img1", PublicId = "1",IsPrincipal = false },
                    new Image { Id = "img2",PublicId = "1", IsPrincipal = false }
                }
            },
            User = new User()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(ownershipRequest);

        // Assert
        Assert.Null(result.Image);
    }

    [Fact]
    public void Map_OwnershipRequest_WithMultipleImages_SelectsPrincipalImage()
    {
        // Arrange
        var ownershipRequest = new OwnershipRequest
        {
            Animal = new Animal
            {
                Images = new List<Image>
                {
                    new Image { Id = "img1", PublicId = "1",IsPrincipal = false, Url = "url1.jpg" },
                    new Image { Id = "img2",PublicId = "1", IsPrincipal = true, Url = "url2.jpg" },
                    new Image { Id = "img3",PublicId = "1", IsPrincipal = false, Url = "url3.jpg" }
                }
            },
            User = new User()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(ownershipRequest);

        // Assert
        Assert.NotNull(result.Image);
        Assert.Equal("img2", result.Image.Id);
        Assert.Equal("url2.jpg", result.Image.Url);
    }

    [Fact]
    public void Map_OwnershipRequest_WithNoImages_ReturnsNullImage()
    {
        // Arrange
        var ownershipRequest = new OwnershipRequest
        {
            Animal = new Animal { Images = new List<Image>() },
            User = new User()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(ownershipRequest);

        // Assert
        Assert.Null(result.Image);
    }

    [Fact]
    public void Map_OwnershipRequest_WithNullRequestInfo_MapsAsNull()
    {
        // Arrange
        var ownershipRequest = new OwnershipRequest
        {
            RequestInfo = null,
            Animal = new Animal { Images = new List<Image>() },
            User = new User()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(ownershipRequest);

        // Assert
        Assert.Null(result.RequestInfo);
    }

    [Fact]
    public void Map_OwnershipRequest_WithNullUpdatedAt_MapsAsNull()
    {
        // Arrange
        var ownershipRequest = new OwnershipRequest
        {
            UpdatedAt = null,
            Animal = new Animal { Images = new List<Image>() },
            User = new User()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(ownershipRequest);

        // Assert
        Assert.Null(result.UpdatedAt);
    }

    [Fact]
    public void Map_ListOfOwnershipRequests_MapsAllCorrectly()
    {
        // Arrange
        var ownershipRequests = new List<OwnershipRequest>
        {
            new OwnershipRequest 
            { 
                Id = "req1", 
                Status = OwnershipStatus.Pending,
                Animal = new Animal { Name = "Max", Images = new List<Image>() },
                User = new User()
            },
            new OwnershipRequest 
            { 
                Id = "req2", 
                Status = OwnershipStatus.Analysing,
                Animal = new Animal { Name = "Bella", Images = new List<Image>() },
                User = new User()
            },
            new OwnershipRequest 
            { 
                Id = "req3", 
                Status = OwnershipStatus.Rejected,
                Animal = new Animal { Name = "Charlie", Images = new List<Image>() },
                User = new User()
            }
        };

        // Act
        var result = _mapper.Map<List<ResUserOwnershipsDto>>(ownershipRequests);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("req1", result[0].Id);
        Assert.Equal("req2", result[1].Id);
        Assert.Equal("req3", result[2].Id);
    }

    #endregion

    #region Animal to ResUserOwnershipsDto Tests

    [Fact]
    public void Map_Animal_ToDto_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var animal = new Animal
        {
            Id = "animal123",
            Name = "Buddy",
            AnimalState = AnimalState.HasOwner,
            Cost = 200.75m,
            OwnershipStartDate = new DateTime(2024, 5, 10, 8, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 10, 20, 16, 45, 0, DateTimeKind.Utc),
            Images = new List<Image>
            {
                new Image { Id = "img1",PublicId = "1", IsPrincipal = true, Url = "buddy.jpg", Description = "Main photo" }
            }
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(animal);

        // Assert
        Assert.Equal("animal123", result.Id);
        Assert.Equal("animal123", result.AnimalId);
        Assert.Equal("Buddy", result.AnimalName);
        Assert.Equal(AnimalState.HasOwner, result.AnimalState);
        Assert.NotNull(result.Image);
        Assert.Equal("img1", result.Image.Id);
        Assert.Equal("buddy.jpg", result.Image.Url);
        Assert.Equal(200.75m, result.Amount);
        Assert.Null(result.OwnershipStatus); // Should be null for owned animals
        Assert.Null(result.RequestInfo); // Should be null for owned animals
        Assert.Equal(new DateTime(2024, 5, 10, 8, 0, 0, DateTimeKind.Utc), result.RequestedAt);
        Assert.Equal(new DateTime(2024, 5, 10, 8, 0, 0, DateTimeKind.Utc), result.ApprovedAt);
        Assert.Equal(new DateTime(2024, 10, 20, 16, 45, 0, DateTimeKind.Utc), result.UpdatedAt);
    }

    [Fact]
    public void Map_Animal_WithNullOwnershipStartDate_UsesCurrentUtcTime()
    {
        // Arrange
        var animal = new Animal
        {
            OwnershipStartDate = null,
            Images = new List<Image>()
        };

        var beforeMapping = DateTime.UtcNow;

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(animal);

        var afterMapping = DateTime.UtcNow;

        // Assert
        Assert.True(result.RequestedAt >= beforeMapping && result.RequestedAt <= afterMapping);
    }

    [Fact]
    public void Map_Animal_OwnershipStatusIsAlwaysNull()
    {
        // Arrange
        var animal = new Animal
        {
            Id = "animal123",
            AnimalState = AnimalState.HasOwner,
            Images = new List<Image>()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(animal);

        // Assert
        Assert.Null(result.OwnershipStatus);
    }

    [Fact]
    public void Map_Animal_RequestInfoIsAlwaysNull()
    {
        // Arrange
        var animal = new Animal
        {
            Id = "animal123",
            Images = new List<Image>()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(animal);

        // Assert
        Assert.Null(result.RequestInfo);
    }

    [Fact]
    public void Map_Animal_WithDifferentAnimalStates_MapsCorrectly()
    {
        // Arrange & Act & Assert
        var states = new[] 
        { 
            AnimalState.Available,
            AnimalState.HasOwner,
            AnimalState.PartiallyFostered,
            AnimalState.TotallyFostered,
            AnimalState.Inactive
        };

        foreach (var state in states)
        {
            var animal = new Animal
            {
                AnimalState = state,
                Images = new List<Image>()
            };

            var result = _mapper.Map<ResUserOwnershipsDto>(animal);

            Assert.Equal(state, result.AnimalState);
        }
    }

    [Fact]
    public void Map_Animal_WithNoPrincipalImage_ReturnsNull()
    {
        // Arrange
        var animal = new Animal
        {
            Images = new List<Image>
            {
                new Image { Id = "img1",PublicId = "1", IsPrincipal = false },
                new Image { Id = "img2",PublicId = "1", IsPrincipal = false }
            }
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(animal);

        // Assert
        Assert.Null(result.Image);
    }

    [Fact]
    public void Map_Animal_WithMultipleImages_SelectsPrincipalImage()
    {
        // Arrange
        var animal = new Animal
        {
            Images = new List<Image>
            {
                new Image { Id = "img1",PublicId = "1", IsPrincipal = false, Url = "url1.jpg" },
                new Image { Id = "img2", PublicId = "1",IsPrincipal = true, Url = "url2.jpg", Description = "Principal" },
                new Image { Id = "img3", PublicId = "1",IsPrincipal = false, Url = "url3.jpg" }
            }
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(animal);

        // Assert
        Assert.NotNull(result.Image);
        Assert.Equal("img2", result.Image.Id);
        Assert.Equal("url2.jpg", result.Image.Url);
        Assert.Equal("Principal", result.Image.Description);
    }

    [Fact]
    public void Map_Animal_WithNoImages_ReturnsNullImage()
    {
        // Arrange
        var animal = new Animal
        {
            Images = new List<Image>()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(animal);

        // Assert
        Assert.Null(result.Image);
    }

    [Fact]
    public void Map_Animal_ApprovedAtEqualsOwnershipStartDate()
    {
        // Arrange
        var ownershipDate = new DateTime(2024, 3, 15, 12, 0, 0, DateTimeKind.Utc);
        var animal = new Animal
        {
            OwnershipStartDate = ownershipDate,
            Images = new List<Image>()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(animal);

        // Assert
        Assert.Equal(ownershipDate, result.ApprovedAt);
        Assert.Equal(ownershipDate, result.RequestedAt);
    }

    [Fact]
    public void Map_Animal_WithNullUpdatedAt_MapsAsNull()
    {
        // Arrange
        var animal = new Animal
        {
            UpdatedAt = null,
            Images = new List<Image>()
        };

        // Act
        var result = _mapper.Map<ResUserOwnershipsDto>(animal);

        // Assert
        Assert.Null(result.UpdatedAt);
    }

    [Fact]
    public void Map_ListOfAnimals_MapsAllCorrectly()
    {
        // Arrange
        var animals = new List<Animal>
        {
            new Animal { Id = "animal1", Name = "Max", AnimalState = AnimalState.HasOwner, Images = new List<Image>() },
            new Animal { Id = "animal2", Name = "Bella", AnimalState = AnimalState.HasOwner, Images = new List<Image>() },
            new Animal { Id = "animal3", Name = "Charlie", AnimalState = AnimalState.HasOwner, Images = new List<Image>() }
        };

        // Act
        var result = _mapper.Map<List<ResUserOwnershipsDto>>(animals);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("animal1", result[0].Id);
        Assert.Equal("animal2", result[1].Id);
        Assert.Equal("animal3", result[2].Id);
        Assert.All(result, dto => Assert.Null(dto.OwnershipStatus));
    }

    #endregion

    #region Configuration Validation Tests

    [Fact]
    public void Mapper_CanMapBothTypes_ToSameDto()
    {
        // Arrange
        var ownershipRequest = new OwnershipRequest
        {
            Id = "req1",
            Status = OwnershipStatus.Pending,
            Animal = new Animal { Name = "Max", Images = new List<Image>() },
            User = new User()
        };

        var animal = new Animal
        {
            Id = "animal1",
            Name = "Bella",
            AnimalState = AnimalState.HasOwner,
            Images = new List<Image>()
        };

        // Act
        var dtoFromRequest = _mapper.Map<ResUserOwnershipsDto>(ownershipRequest);
        var dtoFromAnimal = _mapper.Map<ResUserOwnershipsDto>(animal);

        // Assert
        Assert.NotNull(dtoFromRequest.OwnershipStatus);
        Assert.Null(dtoFromAnimal.OwnershipStatus);
    }

    #endregion
    }
}