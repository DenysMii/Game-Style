using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace GamingPlatform.Services
{
    public class CloudinaryStorageService : ICloudinaryStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _cloudName;
        private readonly string _apiKey;
        private readonly string _apiSecret;

        public CloudinaryStorageService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? configuration["Cloudinary:CloudName"] ?? string.Empty;
            _apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? configuration["Cloudinary:ApiKey"] ?? string.Empty;
            _apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? configuration["Cloudinary:ApiSecret"] ?? string.Empty;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName)
        {
            if (string.IsNullOrWhiteSpace(_cloudName) || string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_apiSecret))
            {
                throw new InvalidOperationException("Cloudinary credentials are not configured.");
            }

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var folder = SanitizeFolderName(folderName);
            var signature = CreateSignature(folder, timestamp);

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);
            content.Add(new StringContent(_apiKey), "api_key");
            content.Add(new StringContent(timestamp), "timestamp");
            content.Add(new StringContent(folder), "folder");
            content.Add(new StringContent(signature), "signature");

            var response = await _httpClient.PostAsync($"https://api.cloudinary.com/v1_1/{_cloudName}/image/upload", content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {responseText}");
            }

            using var document = System.Text.Json.JsonDocument.Parse(responseText);
            return document.RootElement.GetProperty("public_id").GetString() ?? string.Empty;
        }

        private string CreateSignature(string folder, string timestamp)
        {
            var payload = $"folder={folder}&timestamp={timestamp}{_apiSecret}";
            var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(bytes).ToLowerInvariant();
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
