using Microsoft.EntityFrameworkCore;
using GamingPlatform.Data;
using GamingPlatform.Models;

namespace GamingPlatform.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly ApplicationDbContext _context;

        public GameRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retrieves all games ordered by creation date
        public async Task<IEnumerable<Game>> GetAllGamesAsync()
        {
            return await _context.Games
                .Include(g => g.MediaFile)
                .OrderByDescending(g => g.ReleaseDate)
                .ToListAsync();
        }

        // Retrieves a specific game by its ID
        public async Task<Game?> GetGameByIdAsync(int id)
        {
            return await _context.Games
                .Include(g => g.MediaFile)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        // Retrieves paginated games filtered by category
        public async Task<IEnumerable<Game>> GetGamesByCategoryAsync(string category, int page = 1, int pageSize = 12)
        {
            return await _context.Games
                .Include(g => g.MediaFile)
                .Where(g => g.Category.ToLower() == category.ToLower())
                .OrderByDescending(g => g.Rating)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Searches games by name with pagination
        public async Task<IEnumerable<Game>> SearchGamesByNameAsync(string searchTerm, int page = 1, int pageSize = 12)
        {
            return await _context.Games
                .Include(g => g.MediaFile)
                .Where(g => g.Title.ToLower().Contains(searchTerm.ToLower()))
                .OrderByDescending(g => g.Rating)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Gets top rated games limited by count
        public async Task<IEnumerable<Game>> GetTopRatedGamesAsync(int count = 10)
        {
            return await _context.Games
                .Include(g => g.MediaFile)
                .OrderByDescending(g => g.Rating)
                .Take(count)
                .ToListAsync();
        }

        // Gets recently added games limited by count
        public async Task<IEnumerable<Game>> GetRecentGamesAsync(int count = 10)
        {
            return await _context.Games
                .Include(g => g.MediaFile)
                .OrderByDescending(g => g.ReleaseDate)
                .Take(count)
                .ToListAsync();
        }

        // Gets all unique game categories
        public async Task<IEnumerable<string>> GetAllCategoriesAsync()
        {
            return await _context.Games
                .Select(g => g.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        // Gets total count of games
        public async Task<int> GetTotalGamesCountAsync()
        {
            return await _context.Games.CountAsync();
        }

        // Gets count of games in a specific category
        public async Task<int> GetGamesByCategoryCountAsync(string category)
        {
            return await _context.Games
                .Where(g => g.Category.ToLower() == category.ToLower())
                .CountAsync();
        }

        // Gets count of games matching a search term
        public async Task<int> GetSearchResultsCountAsync(string searchTerm)
        {
            return await _context.Games
                .Where(g => g.Title.ToLower().Contains(searchTerm.ToLower()))
                .CountAsync();
        }
    }
}