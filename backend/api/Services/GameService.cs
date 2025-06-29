using GamingPlatform.DTOs;
using GamingPlatform.Models;
using GamingPlatform.Repositories;

namespace GamingPlatform.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;

        public GameService(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        // Gets all games with pagination
        public async Task<PaginatedResult<GameSummaryDto>> GetAllGamesAsync(int page = 1, int pageSize = 12)
        {
            var games = await _gameRepository.GetAllGamesAsync();
            var totalCount = await _gameRepository.GetTotalGamesCountAsync();

            var paginatedGames = games
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToSummaryDto);

            return new PaginatedResult<GameSummaryDto>
            {
                Items = paginatedGames,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // Gets detailed information about a specific game
        public async Task<GameDto?> GetGameByIdAsync(int id)
        {
            var game = await _gameRepository.GetGameByIdAsync(id);
            return game != null ? MapToDetailDto(game) : null;
        }

        // Gets games by category with pagination
        public async Task<PaginatedResult<GameSummaryDto>> GetGamesByCategoryAsync(string category, int page = 1, int pageSize = 12)
        {
            var games = await _gameRepository.GetGamesByCategoryAsync(category, page, pageSize);
            var totalCount = await _gameRepository.GetGamesByCategoryCountAsync(category);

            return new PaginatedResult<GameSummaryDto>
            {
                Items = games.Select(MapToSummaryDto),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // Searches games by name with pagination
        public async Task<PaginatedResult<GameSummaryDto>> SearchGamesByNameAsync(string searchTerm, int page = 1, int pageSize = 12)
        {
            var games = await _gameRepository.SearchGamesByNameAsync(searchTerm, page, pageSize);
            var totalCount = await _gameRepository.GetSearchResultsCountAsync(searchTerm);

            return new PaginatedResult<GameSummaryDto>
            {
                Items = games.Select(MapToSummaryDto),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // Gets top rated games
        public async Task<IEnumerable<GameSummaryDto>> GetTopRatedGamesAsync(int count = 10)
        {
            var games = await _gameRepository.GetTopRatedGamesAsync(count);
            return games.Select(MapToSummaryDto);
        }

        // Gets recently added games
        public async Task<IEnumerable<GameSummaryDto>> GetRecentGamesAsync(int count = 10)
        {
            var games = await _gameRepository.GetRecentGamesAsync(count);
            return games.Select(MapToSummaryDto);
        }

        // Gets all available game categories
        public async Task<IEnumerable<string>> GetAllCategoriesAsync()
        {
            return await _gameRepository.GetAllCategoriesAsync();
        }

        // Maps Game entity to GameSummaryDto for list views
        private static GameSummaryDto MapToSummaryDto(Game game)
        {
            return new GameSummaryDto
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                Banner = game.MediaFile?.Banner ?? string.Empty
            };
        }

        // Maps Game entity to GameDto for detailed view
        private static GameDto MapToDetailDto(Game game)
        {
            return new GameDto
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
                MediaFile = game.MediaFile != null ? new MediaFileDto
                {
                    Icon = game.MediaFile.Icon,
                    FirstMediaFile = game.MediaFile.FirstMediaFile,
                    SecondMediaFile = game.MediaFile.SecondMediaFile,
                    ThirdMediaFile = game.MediaFile.ThirdMediaFile,
                    FourthMediaFile = game.MediaFile.FourthMediaFile
                } : null
            };
        }
    }
}