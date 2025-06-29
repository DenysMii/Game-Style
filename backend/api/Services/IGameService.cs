using GamingPlatform.DTOs;

namespace GamingPlatform.Services
{
    public interface IGameService
    {
        Task<PaginatedResult<GameSummaryDto>> GetAllGamesAsync(int page = 1, int pageSize = 12);
        Task<GameDto?> GetGameByIdAsync(int id);
        Task<PaginatedResult<GameSummaryDto>> GetGamesByCategoryAsync(string category, int page = 1, int pageSize = 12);
        Task<PaginatedResult<GameSummaryDto>> SearchGamesByNameAsync(string searchTerm, int page = 1, int pageSize = 12);
        Task<IEnumerable<GameSummaryDto>> GetTopRatedGamesAsync(int count = 10);
        Task<IEnumerable<GameSummaryDto>> GetRecentGamesAsync(int count = 10);
        Task<IEnumerable<string>> GetAllCategoriesAsync();
    }
}