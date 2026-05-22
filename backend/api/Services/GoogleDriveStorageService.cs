using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
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
            request.SupportsAllDrives = true;

            var uploadResult = await request.UploadAsync();
            if (uploadResult.Status != Google.Apis.Upload.UploadStatus.Completed)
            {
                throw new InvalidOperationException(uploadResult.Exception?.Message ?? "Google Drive upload failed.");
            }

            var permissionRequest = service.Permissions.Create(new Google.Apis.Drive.v3.Data.Permission
            {
                Type = "anyone",
                Role = "reader"
            }, request.ResponseBody.Id);
            permissionRequest.SupportsAllDrives = true;
            await permissionRequest.ExecuteAsync();

            return request.ResponseBody.Id;
        }

        private DriveService CreateDriveService()
        {
            var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? _configuration["GoogleDrive:ClientId"] ?? string.Empty;
            var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? _configuration["GoogleDrive:ClientSecret"] ?? string.Empty;
            var refreshToken = Environment.GetEnvironmentVariable("GOOGLE_REFRESH_TOKEN") ?? _configuration["GoogleDrive:RefreshToken"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new InvalidOperationException("Google Drive OAuth credentials are not configured.");
            }

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId.Trim(),
                    ClientSecret = clientSecret.Trim()
                },
                Scopes = new[] { DriveService.Scope.DriveFile }
            });

            var credential = new UserCredential(flow, "game-style-drive-user", new TokenResponse
            {
                RefreshToken = refreshToken.Trim()
            });

            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Game Style"
            });
        }
    }
}
