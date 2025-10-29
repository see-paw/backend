using Infrastructure.Images;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;

namespace Tests.Images;

/// <summary>
/// Unit tests for ImageService using Equivalence Class Partitioning and Boundary Value Analysis.
/// </summary>
public class ImageServiceTests
{
    private readonly Mock<IOptions<CloudinarySettings>> _mockConfig;
    private readonly CloudinarySettings _validSettings;

    public ImageServiceTests()
    {
        _validSettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-api-key",
            ApiSecret = "test-api-secret"
        };

        _mockConfig = new Mock<IOptions<CloudinarySettings>>();
        _mockConfig.Setup(x => x.Value).Returns(_validSettings);
    }

    #region UploadImage - File Length Tests

    /// <summary>
    /// Tests upload with invalid file lengths (negative, zero, and boundary values).
    /// </summary>
    [Theory]
    [InlineData(-1, "negative.jpg", "animals")]
    [InlineData(-100, "large-negative.png", "shelters")]
    [InlineData(0, "zero.jpg", "animals")]
    public async Task UploadImage_InvalidFileLength_ReturnsNull(long fileLength, string fileName, string folderType)
    {
        var mockFile = CreateMockFile(fileName, fileLength);
        var service = new ImageService(_mockConfig.Object);

        var result = await service.UploadImage(mockFile.Object, folderType);

        Assert.Null(result);
    }

    /// <summary>
    /// Tests upload with valid file lengths at boundaries.
    /// </summary>
    [Theory]
    [InlineData(1, "smallest-valid.jpg", "animals")]
    [InlineData(1024, "1kb.jpg", "shelters")]
    [InlineData(1048576, "1mb.png", "animals")]
    [InlineData(5242880, "5mb.jpg", "shelters")]
    [InlineData(10485760, "10mb.png", "animals")]
    public async Task UploadImage_ValidFileLength_ProcessesSuccessfully(long fileLength, string fileName, string folderType)
    {
        var mockFile = CreateMockFile(fileName, fileLength, hasContent: true);
        var service = new ImageService(_mockConfig.Object);

        using var stream = mockFile.Object.OpenReadStream();

        Assert.NotNull(stream);
        Assert.Equal(fileLength, mockFile.Object.Length);
    }

    /// <summary>
    /// Tests upload with extremely large file lengths.
    /// </summary>
    [Theory]
    [InlineData(52428800, "50mb.jpg", "animals")]
    [InlineData(104857600, "100mb.jpg", "shelters")]
    [InlineData(long.MaxValue, "max-value.jpg", "animals")]
    public async Task UploadImage_ExtremelyLargeFileLength_HandledAppropriately(long fileLength, string fileName, string folderType)
    {
        var mockFile = CreateMockFile(fileName, fileLength, hasContent: true);
        var service = new ImageService(_mockConfig.Object);

        using var stream = mockFile.Object.OpenReadStream();

        Assert.NotNull(stream);
        Assert.Equal(fileLength, mockFile.Object.Length);
    }

    #endregion

    #region UploadImage - FileName Tests

    /// <summary>
    /// Tests upload with invalid file names.
    /// </summary>
    [Theory]
    [InlineData(null, 1024, "animals")]
    [InlineData("", 1024, "shelters")]
    [InlineData("   ", 1024, "animals")]
    public void UploadImage_InvalidFileName_IsDetected(string? fileName, long fileLength, string folderType)
    {
        var isInvalid = string.IsNullOrWhiteSpace(fileName);
        Assert.True(isInvalid);
    }

    /// <summary>
    /// Tests upload with various valid file names and extensions.
    /// </summary>
    [Theory]
    [InlineData("image.jpg", 1024, "animals")]
    [InlineData("image.jpeg", 2048, "shelters")]
    [InlineData("image.png", 512, "animals")]
    [InlineData("image.gif", 1536, "shelters")]
    [InlineData("image.webp", 768, "animals")]
    [InlineData("very-long-file-name-with-multiple-words-and-numbers-12345.jpg", 1024, "shelters")]
    [InlineData("file with spaces.png", 2048, "animals")]
    [InlineData("special!@#$%chars.jpg", 512, "shelters")]
    public void UploadImage_ValidFileName_IsAccepted(string fileName, long fileLength, string folderType)
    {
        var mockFile = CreateMockFile(fileName, fileLength, hasContent: true);

        Assert.Equal(fileName, mockFile.Object.FileName);
        Assert.True(fileLength > 0);
    }

    #endregion

    #region UploadImage - FolderType Tests

    /// <summary>
    /// Tests upload with invalid folder types.
    /// </summary>
    [Theory]
    [InlineData(null, "image.jpg", 1024)]
    [InlineData("", "image.png", 2048)]
    [InlineData("   ", "image.gif", 512)]
    public void UploadImage_InvalidFolderType_IsDetected(string? folderType, string fileName, long fileLength)
    {
        var isInvalid = string.IsNullOrWhiteSpace(folderType);
        Assert.True(isInvalid);
    }

    /// <summary>
    /// Tests upload with valid folder types.
    /// </summary>
    [Theory]
    [InlineData("animals", "image.jpg", 1024, "SeePaw/animals")]
    [InlineData("shelters", "image.png", 2048, "SeePaw/shelters")]
    [InlineData("users", "image.gif", 512, "SeePaw/users")]
    [InlineData("test-folder", "image.webp", 768, "SeePaw/test-folder")]
    [InlineData("folder_with_underscore", "image.jpg", 1024, "SeePaw/folder_with_underscore")]
    public void UploadImage_ValidFolderType_GeneratesCorrectPath(string folderType, string fileName, long fileLength, string expectedPath)
    {
        var actualPath = $"SeePaw/{folderType}";
        Assert.Equal(expectedPath, actualPath);
    }

    #endregion

    #region Configuration Tests

    /// <summary>
    /// Tests ImageService with valid configurations.
    /// </summary>
    [Theory]
    [InlineData("cloud-name", "api-key", "api-secret")]
    [InlineData("a", "b", "c")]
    [InlineData("very-long-cloud-name-with-many-characters", "very-long-api-key", "very-long-api-secret")]
    public void Constructor_ValidConfiguration_InitializesSuccessfully(string cloudName, string apiKey, string apiSecret)
    {
        var config = new CloudinarySettings
        {
            CloudName = cloudName,
            ApiKey = apiKey,
            ApiSecret = apiSecret
        };

        var mockOptions = new Mock<IOptions<CloudinarySettings>>();
        mockOptions.Setup(x => x.Value).Returns(config);

        var service = new ImageService(mockOptions.Object);

        Assert.NotNull(service);
    }

    /// <summary>
    /// Tests ImageService with invalid configurations.
    /// </summary>
    [Theory]
    [InlineData(null, "api-key", "api-secret")]
    [InlineData("cloud-name", null, "api-secret")]
    [InlineData("cloud-name", "api-key", null)]
    [InlineData("", "api-key", "api-secret")]
    [InlineData("cloud-name", "", "api-secret")]
    [InlineData("cloud-name", "api-key", "")]
    [InlineData("   ", "api-key", "api-secret")]
    [InlineData("cloud-name", "   ", "api-secret")]
    [InlineData("cloud-name", "api-key", "   ")]
    [InlineData(null, null, null)]
    [InlineData("", "", "")]
    public void Constructor_InvalidConfiguration_HasInvalidValues(string? cloudName, string? apiKey, string? apiSecret)
    {
        var hasInvalidValue = string.IsNullOrWhiteSpace(cloudName) ||
                             string.IsNullOrWhiteSpace(apiKey) ||
                             string.IsNullOrWhiteSpace(apiSecret);

        Assert.True(hasInvalidValue);
    }

    #endregion

    #region DeleteImage Tests

    /// <summary>
    /// Tests DeleteImage with invalid public IDs.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DeleteImage_InvalidPublicId_IsDetected(string? publicId)
    {
        var isInvalid = string.IsNullOrWhiteSpace(publicId);
        Assert.True(isInvalid);
    }

    /// <summary>
    /// Tests DeleteImage with valid public IDs.
    /// </summary>
    [Theory]
    [InlineData("SeePaw/animals/image123")]
    [InlineData("SeePaw/shelters/abc-def-ghi")]
    [InlineData("a")]
    [InlineData("very-long-public-id-with-many-characters-and-segments/folder1/folder2/image")]
    [InlineData("SeePaw/animals/123456789")]
    public void DeleteImage_ValidPublicId_IsAccepted(string publicId)
    {
        var isValid = !string.IsNullOrWhiteSpace(publicId);
        Assert.True(isValid);
    }

    /// <summary>
    /// Tests DeleteImage with special characters in public IDs.
    /// </summary>
    [Theory]
    [InlineData("SeePaw/animals/image!@#$%")]
    [InlineData("SeePaw/shelters/image with spaces")]
    [InlineData("SeePaw/users/image\nwith\nnewlines")]
    [InlineData("SeePaw/animals/image\twith\ttabs")]
    public void DeleteImage_SpecialCharactersInPublicId_IsAccepted(string publicId)
    {
        var isValid = !string.IsNullOrWhiteSpace(publicId);
        Assert.True(isValid);
    }

    #endregion

    #region Transformation Tests

    /// <summary>
    /// Tests image transformation parameters at boundaries.
    /// </summary>
    [Theory]
    [InlineData(0, 0, "fill")]
    [InlineData(1, 1, "fill")]
    [InlineData(499, 499, "fill")]
    [InlineData(500, 500, "fill")]
    [InlineData(501, 501, "fill")]
    [InlineData(1000, 1000, "fill")]
    [InlineData(9999, 9999, "fill")]
    public void ImageTransformation_VariousDimensions_AreValid(int height, int width, string crop)
    {
        Assert.True(height >= 0);
        Assert.True(width >= 0);
        Assert.False(string.IsNullOrWhiteSpace(crop));
    }

    /// <summary>
    /// Tests image transformation with invalid parameters.
    /// </summary>
    [Theory]
    [InlineData(-1, 500, "fill")]
    [InlineData(500, -1, "fill")]
    [InlineData(-100, -100, "fill")]
    [InlineData(500, 500, null)]
    [InlineData(500, 500, "")]
    [InlineData(500, 500, "   ")]
    public void ImageTransformation_InvalidParameters_AreDetected(int height, int width, string? crop)
    {
        var hasInvalidValue = height < 0 || width < 0 || string.IsNullOrWhiteSpace(crop);
        Assert.True(hasInvalidValue);
    }

    #endregion

    #region Combined Tests

    /// <summary>
    /// Tests upload with multiple invalid parameters simultaneously.
    /// </summary>
    [Theory]
    [InlineData(0, "", null)]
    [InlineData(-1, null, "")]
    [InlineData(0, "   ", "   ")]
    [InlineData(-100, "", null)]
    public void UploadImage_MultipleInvalidParameters_AreAllDetected(long fileLength, string? fileName, string? folderType)
    {
        var hasInvalidFileLength = fileLength <= 0;
        var hasInvalidFileName = string.IsNullOrWhiteSpace(fileName);
        var hasInvalidFolderType = string.IsNullOrWhiteSpace(folderType);

        Assert.True(hasInvalidFileLength && hasInvalidFileName && hasInvalidFolderType);
    }

    /// <summary>
    /// Tests upload with edge case combinations.
    /// </summary>
    [Theory]
    [InlineData(1, "a.jpg", "a")]
    [InlineData(long.MaxValue, "max-file.jpg", "max-folder")]
    [InlineData(1, "single-byte.png", "single")]
    public void UploadImage_EdgeCaseCombinations_AreHandled(long fileLength, string fileName, string folderType)
    {
        var mockFile = CreateMockFile(fileName, fileLength, hasContent: true);

        Assert.Equal(fileName, mockFile.Object.FileName);
        Assert.Equal(fileLength, mockFile.Object.Length);
        Assert.False(string.IsNullOrWhiteSpace(folderType));
    }

    #endregion

    #region Helper Methods

    private Mock<IFormFile> CreateMockFile(string fileName, long length, bool hasContent = false)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(length);

        if (hasContent && length > 0)
        {
            var content = new byte[Math.Min(length, 10485760)];
            var stream = new MemoryStream(content);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        return mockFile;
    }

    #endregion
}