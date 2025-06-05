using Microsoft.AspNetCore.Http;
using Rezqa.Domain.Common.Interfaces;


namespace Rezqa.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _baseDirectory;

    public FileService()
    {
        _baseDirectory = Path.Combine("wwwroot", "uploads");
    }

    public async Task<string> SaveFileAsync(IFormFile file, string directory)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty", nameof(file));

        var uploadPath = Path.Combine(_baseDirectory, directory);
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return the relative path from wwwroot
        return Path.Combine("uploads", directory, fileName).Replace("\\", "/");
    }

    public async Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        var fullPath = Path.Combine(_baseDirectory, filePath.Replace("uploads/", ""));
        if (File.Exists(fullPath))
        {
            await Task.Run(() => File.Delete(fullPath));
        }
    }
}