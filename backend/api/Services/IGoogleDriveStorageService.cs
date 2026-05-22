using Microsoft.AspNetCore.Http;

namespace GamingPlatform.Services
{
    public interface IGoogleDriveStorageService
    {
        Task<string> UploadArchiveAsync(IFormFile file, string gameTitle);
    }
}
