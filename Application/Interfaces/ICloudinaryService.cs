using Application.Images;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

/// <summary>
///     Provides methods for uploading and deleting images to Cloudinary.
/// </summary>
public interface ICloudinaryService
{
    /// <summary>
    ///     Uploads an image to Cloudinary.
    /// </summary>
    /// <param name="file">The image file to upload.</param>
    /// <param name="folderType">The folder where the image will be stored.</param>
    /// <returns>
    ///     The result of the upload, or null if the file is empty.
    /// </returns>
    /// <exception cref="Exception">Thrown if the upload fails.</exception>
    Task<ImageUploadResult?> UploadImage(IFormFile file, string folderType);

    /// <summary>
    ///     Deletes an image from the cloud by its public ID.
    /// </summary>
    /// <param name="publicId">The cloud identifier of the image.</param>
    /// <returns>The result message from the cloud service.</returns>
    /// <exception cref="Exception">Thrown if the deletion fails.</exception>
    Task<string> DeleteImage(string publicId);
}