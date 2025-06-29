using Microsoft.AspNetCore.Mvc;
using GamingPlatform.Services;
using GamingPlatform.DTOs;

namespace GamingPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        // GET: api/games - Returns paginated list of games
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<GameSummaryDto>>> GetAllGames(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 12;

                var result = await _gameService.GetAllGamesAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/games/{id} - Returns detailed information about a specific game
        [HttpGet("{id}")]
        public async Task<ActionResult<GameDto>> GetGameById(int id)
        {
            try
            {
                var game = await _gameService.GetGameByIdAsync(id);
                if (game == null)
                {
                    return NotFound(new { message = "Game not found" });
                }
                return Ok(game);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/games/category/{category} - Returns paginated list of games by category
        [HttpGet("category/{category}")]
        public async Task<ActionResult<PaginatedResult<GameSummaryDto>>> GetGamesByCategory(
            string category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    return BadRequest(new { message = "Category cannot be empty" });
                }

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 12;

                var result = await _gameService.GetGamesByCategoryAsync(category, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/games/search - Returns paginated search results by game name
        [HttpGet("search")]
        public async Task<ActionResult<PaginatedResult<GameSummaryDto>>> SearchGames(
            [FromQuery] string query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { message = "Search query cannot be empty" });
                }

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 12;

                var result = await _gameService.SearchGamesByNameAsync(query, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/games/top-rated - Returns list of top rated games
        [HttpGet("top-rated")]
        public async Task<ActionResult<IEnumerable<GameSummaryDto>>> GetTopRatedGames([FromQuery] int count = 10)
        {
            try
            {
                if (count < 1 || count > 50) count = 10;

                var games = await _gameService.GetTopRatedGamesAsync(count);
                return Ok(games);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/games/recent - Returns list of recently added games
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<GameSummaryDto>>> GetRecentGames(
            [FromQuery] int count = 10)
        {
            try
            {
                if (count < 1 || count > 50) count = 10;

                var games = await _gameService.GetRecentGamesAsync(count);
                return Ok(games);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/games/categories - Returns list of all available game categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            try
            {
                var categories = await _gameService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}