using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Rezqa.Domain.Common.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string directory);
    Task<bool> DeleteFileAsync(string filePath);
}