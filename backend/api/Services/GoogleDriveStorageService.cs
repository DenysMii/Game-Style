using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;

namespace GamingPlatform.Services
{
    public class GoogleDriveStorageService : IGoogleDriveStorageService
    {
        private readonly IConfiguration _configuration;

        public GoogleDriveStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadArchiveAsync(IFormFile file, string gameTitle)
        {
            var service = CreateDriveService();
            var folderId = Environment.GetEnvironmentVariable("GOOGLE_DRIVE_FOLDER_ID") ?? _configuration["GoogleDrive:FolderId"];

            var metadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = string.IsNullOrWhiteSpace(file.FileName) ? $"{gameTitle}.zip" : file.FileName,
                Parents = string.IsNullOrWhiteSpace(folderId) ? null : new List<string> { folderId }
            };

            using var stream = file.OpenReadStream();
            var request = service.Files.Create(metadata, stream, file.ContentType);
            request.Fields = "id, webViewLink";

            var uploadResult = await request.UploadAsync();
            if (uploadResult.Status != Google.Apis.Upload.UploadStatus.Completed)
            {
                throw new InvalidOperationException(uploadResult.Exception?.Message ?? "Google Drive upload failed.");
            }

            await service.Permissions.Create(new Google.Apis.Drive.v3.Data.Permission
            {
                Type = "anyone",
                Role = "reader"
            }, request.ResponseBody.Id).ExecuteAsync();

            return request.ResponseBody.Id;
        }

        private DriveService CreateDriveService()
        {
            var credentialsJson = Environment.GetEnvironmentVariable("GOOGLE_DRIVE_SERVICE_ACCOUNT_JSON") ?? _configuration["GoogleDrive:ServiceAccountJson"];
            var credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_DRIVE_SERVICE_ACCOUNT_PATH") ?? _configuration["GoogleDrive:ServiceAccountPath"];

            GoogleCredential credential;

            if (!string.IsNullOrWhiteSpace(credentialsJson))
            {
                credential = GoogleCredential.FromJson(credentialsJson);
            }
            else if (!string.IsNullOrWhiteSpace(credentialsPath))
            {
                credential = GoogleCredential.FromFile(credentialsPath);
            }
            else
            {
                throw new InvalidOperationException("Google Drive service account credentials are not configured.");
            }

            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential.CreateScoped(DriveService.Scope.DriveFile),
                ApplicationName = "Game Style"
            });
        }
    }
}
