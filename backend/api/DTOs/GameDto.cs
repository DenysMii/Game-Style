namespace GamingPlatform.DTOs
{
    // Full game details DTO used for single game view
    public class GameDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Developer { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public string DownloadLink { get; set; } = string.Empty;
        public string SystemRequirements { get; set; } = string.Empty;
        public MediaFileDto? MediaFile { get; set; }
    }

    // Media files associated with a game
    public class MediaFileDto
    {
        public string Icon { get; set; } = string.Empty;
        public string FirstMediaFile { get; set; } = string.Empty;
        public string SecondMediaFile { get; set; } = string.Empty;
        public string ThirdMediaFile { get; set; } = string.Empty;
        public string FourthMediaFile { get; set; } = string.Empty;
    }

    // Summary DTO used for game listings and search results
    public class GameSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Banner { get; set; } = string.Empty;
    }

    // Pagination wrapper for list responses
    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}