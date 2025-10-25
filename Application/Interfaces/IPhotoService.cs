
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IPhotoService
{
    Task<PhotoUploadResult?> UploadPhoto(IFormFile file, string folderType);
    Task<string> DeletePhoto(string publicId);
}