using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ImageUploadResult = Application.Images.ImageUploadResult;

namespace Infrastructure.Images;

/// <summary>
/// Handles image upload and deletion using Cloudinary.
/// </summary>
public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
        
    /// <summary>
    /// Initializes a new instance of <see cref="CloudinaryService"/> with Cloudinary configuration.
    /// </summary>
    /// <param name="config">The Cloudinary configuration settings.</param>
    public CloudinaryService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret);
        
        _cloudinary = new Cloudinary(account);
    }
    
    /// <summary>
    /// Uploads an image to Cloudinary.
    /// </summary>
    /// <param name="file">The image file to upload.</param>
    /// <param name="folderType">The target folder name in Cloudinary.</param>
    /// <returns>The upload result with the image URL and public ID, or null if empty.</returns>
    /// <exception cref="Exception">Thrown when Cloudinary returns an error.</exception>
    public async Task<ImageUploadResult?> UploadImage(IFormFile file, string folderType)
    {
        if (file.Length <= 0)
        {
            return null;
        }
        
        //using close the stream after executing the method
        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Transformation = new Transformation().Height(500).Width(500).Crop("fill"),
            Folder = $"SeePaw/{folderType}"
        };
            
        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception(uploadResult.Error.Message);
        }

        return new ImageUploadResult
        {
            PublicId = uploadResult.PublicId,
            Url = uploadResult.SecureUrl.AbsoluteUri
        };
    }

    /// <summary>
    /// Deletes an image from Cloudinary.
    /// </summary>
    /// <param name="publicId">The Cloudinary public ID of the image to delete.</param>
    /// <returns>The result message from Cloudinary.</returns>
    /// <exception cref="Exception">Thrown when Cloudinary returns an error.</exception>
    public async Task<string> DeleteImage(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);

        var result = await _cloudinary.DestroyAsync(deleteParams);

        return result.Error != null ? throw new Exception(result.Error.Message) : result.Result;
    }
}