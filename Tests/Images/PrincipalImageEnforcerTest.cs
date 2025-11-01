using Application.Images;
using Application.Interfaces;
using Domain;
using Infrastructure.Images;

namespace Tests.Images;

/// <summary>
/// Unit tests for PrincipalImageEnforcer using equivalence partitioning and boundary value analysis.
/// Tests the EnforceSinglePrincipal method which ensures only one image is marked as principal.
/// </summary>
public class PrincipalImageEnforcerTest
{
    private readonly IPrincipalImageEnforcer _enforcer;

    public PrincipalImageEnforcerTest()
    {
        _enforcer = new PrincipalImageEnforcer();
    }

    #region Helper Methods

    /// <summary>
    /// Creates a test image with the specified IsPrincipal flag.
    /// </summary>
    private static Image CreateImage(bool isPrincipal = false, string? id = null)
    {
        return new Image
        {
            Id = id ?? Guid.NewGuid().ToString(),
            PublicId = $"public-{Guid.NewGuid()}",
            Url = $"https://cloudinary.com/{Guid.NewGuid()}.jpg",
            Description = "Test image",
            IsPrincipal = isPrincipal
        };
    }

    /// <summary>
    /// Creates a collection of images with the specified principal states.
    /// </summary>
    private static ICollection<Image> CreateImageCollection(params bool[] principalStates)
    {
        return principalStates.Select(isPrincipal => CreateImage(isPrincipal)).ToList();
    }

    #endregion

    #region Success Cases - Standard Scenarios

    /// <summary>
    /// Tests enforcement with empty collection.
    /// Equivalence Class: Empty collection (0 images).
    /// Boundary: Minimum collection size.
    /// </summary>
    [Fact]
    public void EnforceSinglePrincipal_EmptyCollection_SetsNewImageAsPrincipal()
    {
        var images = new List<Image>();
        var newImage = CreateImage(isPrincipal: false);

        _enforcer.EnforceSinglePrincipal(images, newImage);

        Assert.True(newImage.IsPrincipal);
        Assert.Empty(images);
    }

    /// <summary>
    /// Tests enforcement with single existing image.
    /// Equivalence Class: Collection with 1 image.
    /// Boundary: Minimum non-empty collection.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EnforceSinglePrincipal_SingleExistingImage_RemovesPrincipalAndSetsNew(bool existingIsPrincipal)
    {
        var existingImage = CreateImage(isPrincipal: existingIsPrincipal);
        var images = new List<Image> { existingImage };
        var newImage = CreateImage(isPrincipal: false);

        _enforcer.EnforceSinglePrincipal(images, newImage);

        Assert.False(existingImage.IsPrincipal);
        Assert.True(newImage.IsPrincipal);
    }

    /// <summary>
    /// Tests enforcement with multiple images, none principal.
    /// Equivalence Class: Collection with N images (N > 1), all non-principal.
    /// </summary>
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void EnforceSinglePrincipal_MultipleImagesNonePrincipal_SetsOnlyNewAsPrincipal(int imageCount)
    {
        var images = Enumerable.Range(0, imageCount)
            .Select(_ => CreateImage(isPrincipal: false))
            .ToList<Image>();
        var newImage = CreateImage(isPrincipal: false);

        _enforcer.EnforceSinglePrincipal(images, newImage);

        Assert.All(images, img => Assert.False(img.IsPrincipal));
        Assert.True(newImage.IsPrincipal);
    }

    /// <summary>
    /// Tests enforcement with multiple images, one principal.
    /// Equivalence Class: Collection with N images (N > 1), exactly one principal.
    /// </summary>
    [Theory]
    [InlineData(2, 0)]
    [InlineData(3, 1)]
    [InlineData(5, 2)]
    [InlineData(10, 9)]
    public void EnforceSinglePrincipal_MultipleImagesOnePrincipal_RemovesPrincipalAndSetsNew(
        int imageCount, int principalIndex)
    {
        var images = new List<Image>();
        for (int i = 0; i < imageCount; i++)
        {
            images.Add(CreateImage(isPrincipal: i == principalIndex));
        }

        var newImage = CreateImage(isPrincipal: false);

        _enforcer.EnforceSinglePrincipal(images, newImage);

        Assert.All(images, img => Assert.False(img.IsPrincipal));
        Assert.True(newImage.IsPrincipal);
    }

    /// <summary>
    /// Tests enforcement with multiple images, all principal.
    /// Equivalence Class: Collection with N images (N > 1), all principal (invalid state).
    /// Edge case: Invalid initial state that should be corrected.
    /// </summary>
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void EnforceSinglePrincipal_MultipleImagesAllPrincipal_RemovesAllAndSetsNew(int imageCount)
    {
        var images = Enumerable.Range(0, imageCount)
            .Select(_ => CreateImage(isPrincipal: true))
            .ToList<Image>();
        var newImage = CreateImage(isPrincipal: false);

        _enforcer.EnforceSinglePrincipal(images, newImage);

        Assert.All(images, img => Assert.False(img.IsPrincipal));
        Assert.True(newImage.IsPrincipal);
    }

    /// <summary>
    /// Tests enforcement with multiple images, mixed principal states.
    /// Equivalence Class: Collection with N images, multiple principal (invalid state).
    /// </summary>
    [Fact]
    public void EnforceSinglePrincipal_MixedPrincipalStates_RemovesAllAndSetsNew()
    {
        var images = CreateImageCollection(true, false, true, false, true);
        var newImage = CreateImage(isPrincipal: false);

        _enforcer.EnforceSinglePrincipal(images, newImage);

        Assert.All(images, img => Assert.False(img.IsPrincipal));
        Assert.True(newImage.IsPrincipal);
    }

    #endregion

    #region Boundary Value Analysis

    /// <summary>
    /// Tests with large collection to verify performance and correctness.
    /// Boundary: Large collection size (stress test).
    /// </summary>
    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    public void EnforceSinglePrincipal_LargeCollection_HandlesCorrectly(int imageCount)
    {
        var images = Enumerable.Range(0, imageCount)
            .Select(i => CreateImage(isPrincipal: i % 10 == 0))
            .ToList<Image>();
        var newImage = CreateImage(isPrincipal: false);

        _enforcer.EnforceSinglePrincipal(images, newImage);

        Assert.All(images, img => Assert.False(img.IsPrincipal));
        Assert.True(newImage.IsPrincipal);
        Assert.Equal(imageCount, images.Count);
    }

    #endregion

    #region New Image States

    /// <summary>
    /// Tests that new image principal state is always set to true, regardless of initial value.
    /// Equivalence Classes: New image with IsPrincipal = false and true.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EnforceSinglePrincipal_NewImageAnyState_AlwaysSetsToPrincipal(bool newImageInitialState)
    {
        var images = CreateImageCollection(true, false, true);
        var newImage = CreateImage(isPrincipal: newImageInitialState);

        _enforcer.EnforceSinglePrincipal(images, newImage);

        Assert.True(newImage.IsPrincipal);
        Assert.All(images, img => Assert.False(img.IsPrincipal));
    }

    #endregion

    #region State Verification

    /// <summary>
    /// Tests that only the new image ends up as principal.
    /// Verifies the core invariant: exactly one principal image after enforcement.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public void EnforceSinglePrincipal_AnyScenario_ResultsInExactlyOnePrincipal(int existingImageCount)
    {
        var images = Enumerable.Range(0, existingImageCount)
            .Select(i => CreateImage(isPrincipal: i % 2 == 0))
            .ToList<Image>();
        var newImage = CreateImage(isPrincipal: false);

        _enforcer.EnforceSinglePrincipal(images, newImage);

        var allImages = images.Append(newImage).ToList();
        var principalCount = allImages.Count(img => img.IsPrincipal);

        Assert.Equal(1, principalCount);
        Assert.True(newImage.IsPrincipal);
    }

    /// <summary>
    /// Tests that collection size is preserved.
    /// Ensures no images are added or removed from the collection.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void EnforceSinglePrincipal_AnyCollection_PreservesCollectionSize(int imageCount)
    {
        var images = Enumerable.Range(0, imageCount)
            .Select(_ => CreateImage())
            .ToList<Image>();
        var originalCount = images.Count;
        var newImage = CreateImage();

        _enforcer.EnforceSinglePrincipal(images, newImage);

        Assert.Equal(originalCount, images.Count);
    }

    /// <summary>
    /// Tests that image references are preserved.
    /// Ensures the method modifies existing objects rather than creating new ones.
    /// </summary>
    [Fact]
    public void EnforceSinglePrincipal_ModifiesExistingImages_PreservesReferences()
    {
        var image1 = CreateImage(isPrincipal: true, id: "img-1");
        var image2 = CreateImage(isPrincipal: false, id: "img-2");
        var image3 = CreateImage(isPrincipal: true, id: "img-3");
        
        var images = new List<Image> { image1, image2, image3 };
        var newImage = CreateImage(isPrincipal: false, id: "new-img");

        _enforcer.EnforceSinglePrincipal(images, newImage);

        Assert.Same(image1, images[0]);
        Assert.Same(image2, images[1]);
        Assert.Same(image3, images[2]);
        Assert.Equal("img-1", images[0].Id);
        Assert.Equal("img-2", images[1].Id);
        Assert.Equal("img-3", images[2].Id);
    }

    #endregion

    #region Idempotency Tests

    /// <summary>
    /// Tests that calling the method multiple times with the same new image is idempotent.
    /// The result should be the same regardless of how many times it's called.
    /// </summary>
    [Fact]
    public void EnforceSinglePrincipal_CalledMultipleTimes_IsIdempotent()
    {
        var images = CreateImageCollection(false, true, false);
        var newImage = CreateImage(isPrincipal: false);

        _enforcer.EnforceSinglePrincipal(images, newImage);
        var firstResult = newImage.IsPrincipal;
        var firstImageStates = images.Select(img => img.IsPrincipal).ToList();

        _enforcer.EnforceSinglePrincipal(images, newImage);
        var secondResult = newImage.IsPrincipal;
        var secondImageStates = images.Select(img => img.IsPrincipal).ToList();

        Assert.Equal(firstResult, secondResult);
        Assert.Equal(firstImageStates, secondImageStates);
        Assert.True(newImage.IsPrincipal);
        Assert.All(images, img => Assert.False(img.IsPrincipal));
    }

    #endregion

    #region Special Scenarios

    /// <summary>
    /// Tests enforcement when new image is already in the collection.
    /// Edge case: The new image to be made principal is already part of the collection.
    /// </summary>
    [Fact]
    public void EnforceSinglePrincipal_NewImageInCollection_StillEnforcesCorrectly()
    {
        var image1 = CreateImage(isPrincipal: true);
        var image2 = CreateImage(isPrincipal: false);
        var newImage = CreateImage(isPrincipal: false);
        
        var images = new List<Image> { image1, image2, newImage };

        _enforcer.EnforceSinglePrincipal(images, newImage);

        Assert.False(image1.IsPrincipal);
        Assert.False(image2.IsPrincipal);
        Assert.True(newImage.IsPrincipal);
    }

    /// <summary>
    /// Tests with alternating principal pattern.
    /// Equivalence Class: Pattern-based principal assignment.
    /// </summary>
    [Theory]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(20)]
    public void EnforceSinglePrincipal_AlternatingPrincipalPattern_RemovesAllAndSetsNew(int imageCount)
    {
        var images = Enumerable.Range(0, imageCount)
            .Select(i => CreateImage(isPrincipal: i % 2 == 0))
            .ToList<Image>();
        var newImage = CreateImage(isPrincipal: false);

        _enforcer.EnforceSinglePrincipal(images, newImage);

        Assert.All(images, img => Assert.False(img.IsPrincipal));
        Assert.True(newImage.IsPrincipal);
    }

    /// <summary>
    /// Tests enforcement with collection containing duplicate image references.
    /// Edge case: Same image object appears multiple times in collection.
    /// </summary>
    [Fact]
    public void EnforceSinglePrincipal_DuplicateImageReferences_HandlesCorrectly()
    {
        var duplicateImage = CreateImage(isPrincipal: true);
        var images = new List<Image> { duplicateImage, duplicateImage, CreateImage(false) };
        var newImage = CreateImage(isPrincipal: false);

        _enforcer.EnforceSinglePrincipal(images, newImage);

        Assert.False(duplicateImage.IsPrincipal);
        Assert.True(newImage.IsPrincipal);
    }

    #endregion

    #region Performance and Scalability

    /// <summary>
    /// Tests that the method completes in reasonable time even with large collections.
    /// Performance boundary test.
    /// </summary>
    [Fact]
    public void EnforceSinglePrincipal_LargeCollection_CompletesQuickly()
    {
        var images = Enumerable.Range(0, 1000)
            .Select(_ => CreateImage(isPrincipal: true))
            .ToList<Image>();
        var newImage = CreateImage(isPrincipal: false);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _enforcer.EnforceSinglePrincipal(images, newImage);
        stopwatch.Stop();

        Assert.True(stopwatch.ElapsedMilliseconds < 100, 
            $"Operation took {stopwatch.ElapsedMilliseconds}ms, expected < 100ms");
        Assert.True(newImage.IsPrincipal);
        Assert.All(images, img => Assert.False(img.IsPrincipal));
    }

    #endregion

    #region Collection Types

    /// <summary>
    /// Tests that the enforcer works with different ICollection implementations.
    /// Ensures the method is not tied to a specific collection type.
    /// </summary>
    [Fact]
    public void EnforceSinglePrincipal_DifferentCollectionTypes_WorksCorrectly()
    {
        var listImages = new List<Image> { CreateImage(true), CreateImage(false) };
        var hashSetImages = new HashSet<Image> { CreateImage(true), CreateImage(false) };
        var arrayImages = new[] { CreateImage(true), CreateImage(false) };

        var newImage1 = CreateImage(false);
        var newImage2 = CreateImage(false);
        var newImage3 = CreateImage(false);

        _enforcer.EnforceSinglePrincipal(listImages, newImage1);
        _enforcer.EnforceSinglePrincipal(hashSetImages, newImage2);
        _enforcer.EnforceSinglePrincipal(arrayImages, newImage3);

        Assert.True(newImage1.IsPrincipal);
        Assert.True(newImage2.IsPrincipal);
        Assert.True(newImage3.IsPrincipal);
        
        Assert.All(listImages, img => Assert.False(img.IsPrincipal));
        Assert.All(hashSetImages, img => Assert.False(img.IsPrincipal));
        Assert.All(arrayImages, img => Assert.False(img.IsPrincipal));
    }

    #endregion
}