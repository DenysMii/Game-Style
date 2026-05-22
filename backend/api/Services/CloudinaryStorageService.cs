using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace GamingPlatform.Services
{
    public class CloudinaryStorageService : ICloudinaryStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryStorageService(IConfiguration configuration)
        {
            var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? configuration["Cloudinary:CloudName"] ?? string.Empty;
            var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? configuration["Cloudinary:ApiKey"] ?? string.Empty;
            var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? configuration["Cloudinary:ApiSecret"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new InvalidOperationException("Cloudinary credentials are not configured.");
            }

            var account = new Account(cloudName.Trim(), apiKey.Trim(), apiSecret.Trim());
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName)
        {
            var folder = SanitizeFolderName(folderName);

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
            }

            return result.PublicId;
        }

        private static string SanitizeFolderName(string folderName)
        {
            var invalidCharacters = Path.GetInvalidFileNameChars();
            var sanitized = new string(folderName
                .Trim()
                .Select(character => invalidCharacters.Contains(character) ? '-' : character)
                .ToArray());

            return string.IsNullOrWhiteSpace(sanitized) ? "uploaded-game" : sanitized;
        }
    }
}
