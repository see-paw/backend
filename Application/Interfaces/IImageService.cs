
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IImageService
{
    Task<ImageUploadResult?> UploadImage(IFormFile file, string folderType);
    Task<string> DeleteImage(string publicId);
}