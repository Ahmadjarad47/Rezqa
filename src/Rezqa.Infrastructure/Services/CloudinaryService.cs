using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Rezqa.Domain.Common.Interfaces;

namespace Rezqa.Infrastructure.Services;

public class CloudinaryService : IFileService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService()
    {
        var cloudName = Environment.GetEnvironmentVariable("cldn") ?? "dhbpbljel";
        var apiKey = Environment.GetEnvironmentVariable("cldid") ?? "819192798292484";
        var apiSecret = Environment.GetEnvironmentVariable("clds") ?? "7yD0nKAY51qOj3d9K78JKkGoAAY";

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> SaveFileAsync(IFormFile file, string directory)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty", nameof(file));

        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = directory,
            UniqueFilename = true,
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        return uploadResult.SecureUrl.ToString();
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        // Extract public ID from the URL
        var publicId = filePath.Split('/').Last().Split('.').First();

        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        return result.Result == "ok";
    }
}