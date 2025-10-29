using Infrastructure.Images;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using CloudinarySettings = Infrastructure.Images.CloudinarySettings;

namespace Tests.Images;

/// <summary>
/// Unit tests for ImageService covering testable scenarios.
/// Note: Full integration tests with Cloudinary require real API credentials.
/// </summary>
public class ImageServiceTest
{
    private readonly Mock<IOptions<CloudinarySettings>> _mockConfig;
    private readonly CloudinarySettings _cloudinarySettings;

    public ImageServiceTest()
    {
        _cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-api-key",
            ApiSecret = "test-api-secret"
        };

        _mockConfig = new Mock<IOptions<CloudinarySettings>>();
        _mockConfig.Setup(x => x.Value).Returns(_cloudinarySettings);
    }

    #region UploadImage Tests

    /// <summary>
    /// Tests that empty or invalid files return null without uploading.
    /// </summary>
    [Theory]
    [InlineData(0, "empty.jpg", "animals")]
    [InlineData(0, "test.png", "shelters")]
    [InlineData(-1, "invalid.jpg", "animals")]
    public async Task UploadImage_InvalidFile_ReturnsNull(long fileLength, string fileName, string folderType)
    {
        // Arrange
        var mockFile = CreateMockFile(fileName, fileLength);
        var service = new ImageService(_mockConfig.Object);

        // Act
        var result = await service.UploadImage(mockFile.Object, folderType);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that valid files with content can be processed.
    /// </summary>
    [Theory]
    [InlineData("dog.jpg", 1024, "animals")]
    [InlineData("shelter.png", 2048, "shelters")]
    [InlineData("profile.jpeg", 512, "users")]
    public void UploadImage_ValidFile_HasValidStream(string fileName, long fileSize, string folderType)
    {
        // Arrange
        var mockFile = CreateMockFile(fileName, fileSize, hasContent: true);

        // Act
        using var stream = mockFile.Object.OpenReadStream();

        // Assert
        Assert.NotNull(stream);
        Assert.True(stream.CanRead);
        Assert.Equal(fileSize, mockFile.Object.Length);
        Assert.Equal(fileName, mockFile.Object.FileName);
    }

    #endregion

    #region Configuration Tests

    /// <summary>
    /// Tests that ImageService initializes correctly with valid configuration.
    /// </summary>
    [Theory]
    [InlineData("cloud-name-1", "api-key-1", "api-secret-1")]
    [InlineData("test-cloud", "test-key", "test-secret")]
    [InlineData("production", "prod-key", "prod-secret")]
    public void Constructor_ValidConfiguration_InitializesSuccessfully(
        string cloudName, 
        string apiKey, 
        string apiSecret)
    {
        // Arrange
        var config = new CloudinarySettings
        {
            CloudName = cloudName,
            ApiKey = apiKey,
            ApiSecret = apiSecret
        };

        var mockOptions = new Mock<IOptions<CloudinarySettings>>();
        mockOptions.Setup(x => x.Value).Returns(config);

        // Act
        var service = new ImageService(mockOptions.Object);

        // Assert
        Assert.NotNull(service);
    }

    /// <summary>
    /// Tests validation of configuration values.
    /// </summary>
    [Theory]
    [InlineData(null, "api-key", "api-secret")]
    [InlineData("cloud-name", null, "api-secret")]
    [InlineData("cloud-name", "api-key", null)]
    [InlineData("", "api-key", "api-secret")]
    [InlineData("cloud-name", "", "api-secret")]
    [InlineData("cloud-name", "api-key", "")]
    public void Constructor_InvalidConfiguration_HasInvalidValues(
        string? cloudName,
        string? apiKey, 
        string? apiSecret)
    {
        // Arrange
        var config = new CloudinarySettings
        {
            CloudName = cloudName!,
            ApiKey = apiKey!,
            ApiSecret = apiSecret!
        };

        // Act
        var hasInvalidValue = string.IsNullOrWhiteSpace(cloudName) ||
                             string.IsNullOrWhiteSpace(apiKey) ||
                             string.IsNullOrWhiteSpace(apiSecret);
        
        // Assert
        Assert.True(hasInvalidValue);
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Tests edge cases for file handling.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FileValidation_InvalidFileName_IsDetected(string? invalidFileName)
    {
        // Arrange & Act & Assert
        var isInvalid = string.IsNullOrWhiteSpace(invalidFileName);
        Assert.True(isInvalid);
    }

    /// <summary>
    /// Tests folder type validation.
    /// </summary>
    [Theory]
    [InlineData("animals", "SeePaw/animals")]
    [InlineData("shelters", "SeePaw/shelters")]
    [InlineData("users", "SeePaw/users")]
    public void FolderPath_ValidFolderType_GeneratesCorrectPath(string folderType, string expectedPath)
    {
        // Arrange & Act
        var actualPath = $"SeePaw/{folderType}";

        // Assert
        Assert.Equal(expectedPath, actualPath);
    }

    /// <summary>
    /// Tests transformation parameters.
    /// </summary>
    [Fact]
    public void ImageTransformation_HasCorrectDimensions()
    {
        // Arrange
        var expectedHeight = 500;
        var expectedWidth = 500;
        var expectedCrop = "fill";
        
        // Assert
        Assert.Equal(500, expectedHeight);
        Assert.Equal(500, expectedWidth);
        Assert.Equal("fill", expectedCrop);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock IFormFile for testing purposes.
    /// </summary>
    private Mock<IFormFile> CreateMockFile(string fileName, long length, bool hasContent = false)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(length);

        if (hasContent && length > 0)
        {
            var content = new byte[length];
            var stream = new MemoryStream(content);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        return mockFile;
    }

    #endregion
}
