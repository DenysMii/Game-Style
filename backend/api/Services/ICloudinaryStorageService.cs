using Microsoft.AspNetCore.Http;

namespace GamingPlatform.Services
{
    public interface ICloudinaryStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName);
    }
}
