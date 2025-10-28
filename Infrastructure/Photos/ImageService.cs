using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ImageUploadResult = Application.ImageUploadResult;

namespace Infrastructure.Photos;

public class ImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public ImageService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret);
        
        _cloudinary = new Cloudinary(account);
    }
    
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

    public async Task<string> DeleteImage(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);

        var result = await _cloudinary.DestroyAsync(deleteParams);

        return result.Error != null ? throw new Exception(result.Error.Message) : result.Result;
    }
}