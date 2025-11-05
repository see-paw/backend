using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;

namespace Tests.AnimalsTests;

/// <summary>
/// Unit tests for AnimalImageLinker using equivalence partitioning and boundary value analysis.
/// Tests the Link method which associates an image with an animal entity.
/// </summary>
public class AnimalImageLinkerTest
{
    private readonly IImageOwnerLinker<Animal> _linker;

    public AnimalImageLinkerTest()
    {
        _linker = new AnimalImageLinker();
    }

    #region Helper Methods

    /// <summary>
    /// Creates a test animal with minimal required properties.
    /// </summary>
    private static Animal CreateAnimal(string id, int initialImageCount = 0)
    {
        var animal = new Animal
        {
            Id = id,
            Name = "Test Animal",
            AnimalState = AnimalState.Available,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50.00m,
            ShelterId = Guid.NewGuid().ToString(),
            BreedId = Guid.NewGuid().ToString(),
            Images = new List<Image>()
        };

        for (int i = 0; i < initialImageCount; i++)
        {
            animal.Images.Add(CreateImage($"existing-{i}"));
        }

        return animal;
    }

    /// <summary>
    /// Creates a test image with optional animal ID.
    /// </summary>
    private static Image CreateImage(string? publicId = null, string? animalId = null)
    {
        return new Image
        {
            Id = Guid.NewGuid().ToString(),
            PublicId = publicId ?? $"public-{Guid.NewGuid()}",
            Url = $"https://cloudinary.com/{Guid.NewGuid()}.jpg",
            Description = "Test image",
            IsPrincipal = false,
            AnimalId = animalId
        };
    }

    #endregion

    #region Success Cases - Basic Linking

    /// <summary>
    /// Tests linking image to animal with empty image collection.
    /// Equivalence Class: Animal with 0 existing images.
    /// Boundary: Minimum collection size.
    /// </summary>
    [Fact]
    public void Link_AnimalWithNoImages_LinksImageCorrectly()
    {
        var animalId = "animal-1";
        var animal = CreateAnimal(animalId, initialImageCount: 0);
        var image = CreateImage();

        _linker.Link(animal, image, animalId);

        Assert.Equal(animalId, image.AnimalId);
        Assert.Single(animal.Images);
        Assert.Contains(image, animal.Images);
    }

    /// <summary>
    /// Tests linking image to animal with one existing image.
    /// Equivalence Class: Animal with 1 existing image.
    /// Boundary: Minimum non-empty collection.
    /// </summary>
    [Fact]
    public void Link_AnimalWithOneImage_AddsSecondImage()
    {
        var animalId = "animal-2";
        var animal = CreateAnimal(animalId, initialImageCount: 1);
        var newImage = CreateImage();

        _linker.Link(animal, newImage, animalId);

        Assert.Equal(animalId, newImage.AnimalId);
        Assert.Equal(2, animal.Images.Count);
        Assert.Contains(newImage, animal.Images);
    }

    /// <summary>
    /// Tests linking multiple images sequentially to the same animal.
    /// Equivalence Class: Multiple sequential link operations.
    /// </summary>
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void Link_MultipleImagesSequentially_AllLinkedCorrectly(int imagesToAdd)
    {
        var animalId = "animal-multi";
        var animal = CreateAnimal(animalId, initialImageCount: 0);
        var images = new List<Image>();

        for (int i = 0; i < imagesToAdd; i++)
        {
            var image = CreateImage($"image-{i}");
            images.Add(image);
            _linker.Link(animal, image, animalId);
        }

        Assert.Equal(imagesToAdd, animal.Images.Count);
        Assert.All(images, img => Assert.Equal(animalId, img.AnimalId));
        Assert.All(images, img => Assert.Contains(img, animal.Images));
    }

    #endregion

    #region Entity ID Variations

    /// <summary>
    /// Tests linking with various entity ID formats.
    /// Equivalence Classes: Different valid ID formats.
    /// </summary>
    [Theory]
    [InlineData("animal-123")]
    [InlineData("animal-with-dashes")]
    [InlineData("animal_with_underscores")]
    [InlineData("ANIMAL-UPPERCASE")]
    [InlineData("123456")]
    public void Link_VariousEntityIdFormats_SetsAnimalIdCorrectly(string entityId)
    {
        var animal = CreateAnimal(entityId);
        var image = CreateImage();

        _linker.Link(animal, image, entityId);

        Assert.Equal(entityId, image.AnimalId);
        Assert.Contains(image, animal.Images);
    }

    /// <summary>
    /// Tests linking with GUID-formatted entity ID.
    /// Common real-world scenario.
    /// </summary>
    [Fact]
    public void Link_GuidFormattedEntityId_SetsAnimalIdCorrectly()
    {
        var entityId = Guid.NewGuid().ToString();
        var animal = CreateAnimal(entityId);
        var image = CreateImage();

        _linker.Link(animal, image, entityId);

        Assert.Equal(entityId, image.AnimalId);
        Assert.Contains(image, animal.Images);
    }

    /// <summary>
    /// Tests that entity ID parameter is used, not the animal's ID property.
    /// Verifies the method uses the provided entityId parameter.
    /// </summary>
    [Fact]
    public void Link_DifferentEntityIdFromAnimalId_UsesProvidedEntityId()
    {
        var animalId = "animal-original";
        var providedEntityId = "animal-provided";
        var animal = CreateAnimal(animalId);
        var image = CreateImage();

        _linker.Link(animal, image, providedEntityId);

        Assert.Equal(providedEntityId, image.AnimalId);
        Assert.Contains(image, animal.Images);
    }

    #endregion

    #region Image State Preservation

    /// <summary>
    /// Tests that linking preserves existing image properties.
    /// Ensures only AnimalId is modified.
    /// </summary>
    [Fact]
    public void Link_PreservesImageProperties_OnlyModifiesAnimalId()
    {
        var animalId = "animal-preserve";
        var animal = CreateAnimal(animalId);
        var image = new Image
        {
            Id = "img-123",
            PublicId = "public-123",
            Url = "https://example.com/image.jpg",
            Description = "Original description",
            IsPrincipal = true,
            AnimalId = null
        };

        var originalId = image.Id;
        var originalPublicId = image.PublicId;
        var originalUrl = image.Url;
        var originalDescription = image.Description;
        var originalIsPrincipal = image.IsPrincipal;

        _linker.Link(animal, image, animalId);

        Assert.Equal(originalId, image.Id);
        Assert.Equal(originalPublicId, image.PublicId);
        Assert.Equal(originalUrl, image.Url);
        Assert.Equal(originalDescription, image.Description);
        Assert.Equal(originalIsPrincipal, image.IsPrincipal);
        Assert.Equal(animalId, image.AnimalId);
    }

    /// <summary>
    /// Tests linking image that already has an AnimalId.
    /// Equivalence Class: Image with existing AnimalId (overwrite scenario).
    /// </summary>
    [Fact]
    public void Link_ImageWithExistingAnimalId_OverwritesAnimalId()
    {
        var oldAnimalId = "old-animal";
        var newAnimalId = "new-animal";
        var animal = CreateAnimal(newAnimalId);
        var image = CreateImage(animalId: oldAnimalId);

        Assert.Equal(oldAnimalId, image.AnimalId);

        _linker.Link(animal, image, newAnimalId);

        Assert.Equal(newAnimalId, image.AnimalId);
        Assert.Contains(image, animal.Images);
    }

    #endregion

    #region Collection State Tests

    /// <summary>
    /// Tests that linking to animal with existing images preserves existing images.
    /// Ensures the collection is expanded, not replaced.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void Link_AnimalWithExistingImages_PreservesExistingImages(int existingImageCount)
    {
        var animalId = "animal-existing";
        var animal = CreateAnimal(animalId, initialImageCount: existingImageCount);
        var existingImages = animal.Images.ToList();
        var newImage = CreateImage();

        _linker.Link(animal, newImage, animalId);

        Assert.Equal(existingImageCount + 1, animal.Images.Count);
        Assert.All(existingImages, img => Assert.Contains(img, animal.Images));
        Assert.Contains(newImage, animal.Images);
    }

    /// <summary>
    /// Tests that the same image reference is added to the collection.
    /// Verifies no image cloning occurs.
    /// </summary>
    [Fact]
    public void Link_AddsImageReference_NotCopy()
    {
        var animalId = "animal-ref";
        var animal = CreateAnimal(animalId);
        var image = CreateImage();

        _linker.Link(animal, image, animalId);

        Assert.Same(image, animal.Images.First());
    }

    /// <summary>
    /// Tests linking the same image instance multiple times.
    /// Edge case: Duplicate image reference.
    /// </summary>
    [Fact]
    public void Link_SameImageMultipleTimes_AddsMultipleReferences()
    {
        var animalId = "animal-duplicate";
        var animal = CreateAnimal(animalId);
        var image = CreateImage();

        _linker.Link(animal, image, animalId);
        _linker.Link(animal, image, animalId);
        _linker.Link(animal, image, animalId);

        Assert.Equal(3, animal.Images.Count);
        Assert.All(animal.Images, img => Assert.Same(image, img));
    }

    #endregion

    #region Boundary Value Analysis

    /// <summary>
    /// Tests linking with very long entity ID.
    /// Boundary: Maximum practical ID length.
    /// </summary>
    [Fact]
    public void Link_VeryLongEntityId_SetsCorrectly()
    {
        var longEntityId = new string('a', 500);
        var animal = CreateAnimal(longEntityId);
        var image = CreateImage();

        _linker.Link(animal, image, longEntityId);

        Assert.Equal(longEntityId, image.AnimalId);
        Assert.Contains(image, animal.Images);
    }

    /// <summary>
    /// Tests linking with single character entity ID.
    /// Boundary: Minimum practical ID length.
    /// </summary>
    [Fact]
    public void Link_SingleCharacterEntityId_SetsCorrectly()
    {
        var entityId = "a";
        var animal = CreateAnimal(entityId);
        var image = CreateImage();

        _linker.Link(animal, image, entityId);

        Assert.Equal(entityId, image.AnimalId);
        Assert.Contains(image, animal.Images);
    }

    /// <summary>
    /// Tests linking many images to verify scalability.
    /// Boundary: Large number of images.
    /// </summary>
    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(200)]
    public void Link_ManyImages_HandlesCorrectly(int imageCount)
    {
        var animalId = "animal-many";
        var animal = CreateAnimal(animalId);

        for (int i = 0; i < imageCount; i++)
        {
            var image = CreateImage($"img-{i}");
            _linker.Link(animal, image, animalId);
        }

        Assert.Equal(imageCount, animal.Images.Count);
        Assert.All(animal.Images, img => Assert.Equal(animalId, img.AnimalId));
    }

    #endregion

    #region State Verification

    /// <summary>
    /// Tests that linking creates correct bidirectional relationship indicators.
    /// Verifies the core linking behavior.
    /// </summary>
    [Fact]
    public void Link_CreatesCorrectRelationship_ImageToAnimal()
    {
        var animalId = "animal-rel";
        var animal = CreateAnimal(animalId);
        var image = CreateImage();

        _linker.Link(animal, image, animalId);

        Assert.Equal(animalId, image.AnimalId);
        Assert.Contains(image, animal.Images);
        Assert.Single(animal.Images, img => img.Id == image.Id);
    }

    /// <summary>
    /// Tests that multiple links maintain collection integrity.
    /// Ensures each image has correct AnimalId.
    /// </summary>
    [Fact]
    public void Link_MultipleImages_AllHaveCorrectAnimalId()
    {
        var animalId = "animal-multi-verify";
        var animal = CreateAnimal(animalId);
        var images = new[]
        {
            CreateImage("img-1"),
            CreateImage("img-2"),
            CreateImage("img-3")
        };

        foreach (var image in images)
        {
            _linker.Link(animal, image, animalId);
        }

        Assert.Equal(3, animal.Images.Count);
        Assert.All(animal.Images, img => Assert.Equal(animalId, img.AnimalId));
    }

    #endregion

    #region Order Preservation

    /// <summary>
    /// Tests that images are added in the order they are linked.
    /// Verifies insertion order preservation.
    /// </summary>
    [Fact]
    public void Link_MultipleImages_PreservesInsertionOrder()
    {
        var animalId = "animal-order";
        var animal = CreateAnimal(animalId);
        var image1 = CreateImage("first");
        var image2 = CreateImage("second");
        var image3 = CreateImage("third");

        _linker.Link(animal, image1, animalId);
        _linker.Link(animal, image2, animalId);
        _linker.Link(animal, image3, animalId);

        var imageList = animal.Images.ToList();
        Assert.Equal("first", imageList[0].PublicId);
        Assert.Equal("second", imageList[1].PublicId);
        Assert.Equal("third", imageList[2].PublicId);
    }

    #endregion

    #region Image Properties Combinations

    /// <summary>
    /// Tests linking images with various IsPrincipal states.
    /// Equivalence Classes: IsPrincipal true/false.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Link_ImageWithVariousPrincipalStates_LinksCorrectly(bool isPrincipal)
    {
        var animalId = "animal-principal";
        var animal = CreateAnimal(animalId);
        var image = CreateImage();
        image.IsPrincipal = isPrincipal;

        _linker.Link(animal, image, animalId);

        Assert.Equal(animalId, image.AnimalId);
        Assert.Equal(isPrincipal, image.IsPrincipal);
        Assert.Contains(image, animal.Images);
    }

    /// <summary>
    /// Tests linking images with null descriptions.
    /// Edge case: Optional property being null.
    /// </summary>
    [Fact]
    public void Link_ImageWithNullDescription_LinksCorrectly()
    {
        var animalId = "animal-null-desc";
        var animal = CreateAnimal(animalId);
        var image = CreateImage();
        image.Description = null;

        _linker.Link(animal, image, animalId);

        Assert.Equal(animalId, image.AnimalId);
        Assert.Null(image.Description);
        Assert.Contains(image, animal.Images);
    }

    #endregion

    #region Special Characters in IDs

    /// <summary>
    /// Tests linking with entity IDs containing special characters.
    /// Equivalence Class: IDs with special characters.
    /// </summary>
    [Theory]
    [InlineData("animal@123")]
    [InlineData("animal#456")]
    [InlineData("animal$789")]
    [InlineData("animal%abc")]
    [InlineData("animal&xyz")]
    public void Link_EntityIdWithSpecialCharacters_SetsCorrectly(string entityId)
    {
        var animal = CreateAnimal(entityId);
        var image = CreateImage();

        _linker.Link(animal, image, entityId);

        Assert.Equal(entityId, image.AnimalId);
        Assert.Contains(image, animal.Images);
    }

    /// <summary>
    /// Tests linking with entity ID containing Unicode characters.
    /// Edge case: Non-ASCII characters.
    /// </summary>
    [Theory]
    [InlineData("animal-中文")]
    [InlineData("animal-مرحبا")]
    [InlineData("animal-🐕")]
    public void Link_EntityIdWithUnicodeCharacters_SetsCorrectly(string entityId)
    {
        var animal = CreateAnimal(entityId);
        var image = CreateImage();

        _linker.Link(animal, image, entityId);

        Assert.Equal(entityId, image.AnimalId);
        Assert.Contains(image, animal.Images);
    }

    #endregion

    #region Idempotency Tests

    /// <summary>
    /// Tests that linking the same image twice doesn't change the AnimalId after first link.
    /// Verifies AnimalId is set consistently.
    /// </summary>
    [Fact]
    public void Link_SameImageTwiceWithSameEntityId_AnimalIdRemainsConsistent()
    {
        var animalId = "animal-idempotent";
        var animal = CreateAnimal(animalId);
        var image = CreateImage();

        _linker.Link(animal, image, animalId);
        var firstAnimalId = image.AnimalId;

        _linker.Link(animal, image, animalId);
        var secondAnimalId = image.AnimalId;

        Assert.Equal(firstAnimalId, secondAnimalId);
        Assert.Equal(animalId, image.AnimalId);
    }

    #endregion

    #region Performance Tests

    /// <summary>
    /// Tests that linking operation completes quickly even with many existing images.
    /// Performance boundary test.
    /// </summary>
    [Fact]
    public void Link_AnimalWithManyExistingImages_CompletesQuickly()
    {
        var animalId = "animal-perf";
        var animal = CreateAnimal(animalId, initialImageCount: 1000);
        var newImage = CreateImage();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _linker.Link(animal, newImage, animalId);
        stopwatch.Stop();

        Assert.True(stopwatch.ElapsedMilliseconds < 50,
            $"Operation took {stopwatch.ElapsedMilliseconds}ms, expected < 50ms");
        Assert.Equal(1001, animal.Images.Count);
    }

    #endregion

    #region Collection Type Tests

    /// <summary>
    /// Tests that linking works regardless of the collection implementation.
    /// Verifies method works with ICollection interface.
    /// </summary>
    [Fact]
    public void Link_DifferentCollectionImplementations_WorksCorrectly()
    {
        var animalId = "animal-collection";
        
        var animalWithList = CreateAnimal(animalId);
        animalWithList.Images = new List<Image>();
        
        var animalWithHashSet = CreateAnimal(animalId);
        animalWithHashSet.Images = new HashSet<Image>();

        var image1 = CreateImage("img-1");
        var image2 = CreateImage("img-2");

        _linker.Link(animalWithList, image1, animalId);
        _linker.Link(animalWithHashSet, image2, animalId);

        Assert.Contains(image1, animalWithList.Images);
        Assert.Contains(image2, animalWithHashSet.Images);
        Assert.Equal(animalId, image1.AnimalId);
        Assert.Equal(animalId, image2.AnimalId);
    }

    #endregion

    #region Integration Scenarios

    /// <summary>
    /// Tests a realistic scenario: adding multiple images to a new animal.
    /// Integration test covering typical use case.
    /// </summary>
    [Fact]
    public void Link_RealisticScenario_AddingMultipleImagesToNewAnimal()
    {
        var animalId = Guid.NewGuid().ToString();
        var animal = CreateAnimal(animalId, initialImageCount: 0);
        
        var principalImage = CreateImage("principal");
        principalImage.IsPrincipal = true;
        
        var secondaryImages = new[]
        {
            CreateImage("secondary-1"),
            CreateImage("secondary-2"),
            CreateImage("secondary-3")
        };

        _linker.Link(animal, principalImage, animalId);
        foreach (var img in secondaryImages)
        {
            _linker.Link(animal, img, animalId);
        }

        Assert.Equal(4, animal.Images.Count);
        Assert.Contains(principalImage, animal.Images);
        Assert.All(secondaryImages, img => Assert.Contains(img, animal.Images));
        Assert.All(animal.Images, img => Assert.Equal(animalId, img.AnimalId));
    }

    #endregion
}