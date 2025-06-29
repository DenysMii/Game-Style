using GamingPlatform.Models;

namespace GamingPlatform.Repositories
{
    public interface IGameRepository
    {
        // Basic CRUD operations
        Task<IEnumerable<Game>> GetAllGamesAsync();
        Task<Game?> GetGameByIdAsync(int id);
        
        // Category and search operations
        Task<IEnumerable<Game>> GetGamesByCategoryAsync(string category, int page = 1, int pageSize = 12);
        Task<IEnumerable<Game>> SearchGamesByNameAsync(string searchTerm, int page = 1, int pageSize = 12);
        
        // Featured games operations
        Task<IEnumerable<Game>> GetTopRatedGamesAsync(int count = 10);
        Task<IEnumerable<Game>> GetRecentGamesAsync(int count = 10);
        Task<IEnumerable<string>> GetAllCategoriesAsync();
        
        // Count operations for pagination
        Task<int> GetTotalGamesCountAsync();
        Task<int> GetGamesByCategoryCountAsync(string category);
        Task<int> GetSearchResultsCountAsync(string searchTerm);
    }
}