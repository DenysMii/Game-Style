using GamingPlatform.Data;
using GamingPlatform.DTOs;
using GamingPlatform.Models;
using GamingPlatform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamingPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemberGamesController : ControllerBase
    {
        private static readonly HashSet<string> AllowedArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".zip",
            ".rar",
            ".7z",
            ".tar",
            ".gz",
            ".bz2"
        };

        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryStorageService _cloudinaryStorageService;
        private readonly IGoogleDriveStorageService _googleDriveStorageService;
        private readonly ILogger<MemberGamesController> _logger;

        public MemberGamesController(
            ApplicationDbContext context,
            ICloudinaryStorageService cloudinaryStorageService,
            IGoogleDriveStorageService googleDriveStorageService,
            ILogger<MemberGamesController> logger)
        {
            _context = context;
            _cloudinaryStorageService = cloudinaryStorageService;
            _googleDriveStorageService = googleDriveStorageService;
            _logger = logger;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(1_500_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 1_500_000_000)]
        public async Task<ActionResult<GameDto>> UploadGame([FromForm] UploadGameRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(existingUser =>
                existingUser.Id == request.UserId && existingUser.Username == request.Username);

            if (user == null)
            {
                return Unauthorized(new { message = "Користувача не знайдено. Увійдіть ще раз." });
            }

            if (request.GameplayImages.Count > 4)
            {
                return BadRequest(new { message = "Можна завантажити не більше 4 зображень геймплею." });
            }

            if (!IsImage(request.Icon) || request.GameplayImages.Any(file => !IsImage(file)))
            {
                return BadRequest(new { message = "Іконка та зображення геймплею мають бути файлами зображень." });
            }

            var archiveExtension = Path.GetExtension(request.Archive.FileName);
            if (string.IsNullOrWhiteSpace(archiveExtension) || !AllowedArchiveExtensions.Contains(archiveExtension))
            {
                return BadRequest(new { message = "Архів має бути у форматі .zip, .rar, .7z, .tar, .gz або .bz2." });
            }

            try
            {
                var title = request.Title.Trim();
                var iconPublicId = await _cloudinaryStorageService.UploadImageAsync(request.Icon, title);
                var gameplayPublicIds = new List<string>();

                foreach (var image in request.GameplayImages)
                {
                    gameplayPublicIds.Add(await _cloudinaryStorageService.UploadImageAsync(image, title));
                }

                var archiveFileId = await _googleDriveStorageService.UploadArchiveAsync(request.Archive, title);

                var game = new Game
                {
                    Title = title,
                    Description = request.Description.Trim(),
                    Developer = user.Username,
                    ReleaseDate = DateTime.UtcNow,
                    Category = request.Category.Trim(),
                    Rating = 0,
                    DownloadLink = archiveFileId,
                    SystemRequirements = request.SystemRequirements.Trim()
                };

                _context.Games.Add(game);
                await _context.SaveChangesAsync();

                var mediaFile = new MediaFile
                {
                    GameID = game.Id,
                    Banner = gameplayPublicIds.FirstOrDefault() ?? iconPublicId,
                    Icon = iconPublicId,
                    FirstMediaFile = gameplayPublicIds.ElementAtOrDefault(0) ?? string.Empty,
                    SecondMediaFile = gameplayPublicIds.ElementAtOrDefault(1) ?? string.Empty,
                    ThirdMediaFile = gameplayPublicIds.ElementAtOrDefault(2) ?? string.Empty,
                    FourthMediaFile = gameplayPublicIds.ElementAtOrDefault(3) ?? string.Empty
                };

                _context.MediaFiles.Add(mediaFile);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GamesController.GetGameById), "Games", new { id = game.Id }, new GameDto
                {
                    Id = game.Id,
                    Title = game.Title,
                    Description = game.Description,
                    Developer = game.Developer,
                    ReleaseDate = game.ReleaseDate,
                    Category = game.Category,
                    Rating = game.Rating,
                    DownloadLink = game.DownloadLink,
                    SystemRequirements = game.SystemRequirements,
                    MediaFile = new MediaFileDto
                    {
                        Icon = mediaFile.Icon,
                        FirstMediaFile = mediaFile.FirstMediaFile,
                        SecondMediaFile = mediaFile.SecondMediaFile,
                        ThirdMediaFile = mediaFile.ThirdMediaFile,
                        FourthMediaFile = mediaFile.FourthMediaFile
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Member game upload failed.");
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected member game upload failure.");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private static bool IsImage(IFormFile file)
        {
            return !string.IsNullOrWhiteSpace(file.ContentType) && file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
