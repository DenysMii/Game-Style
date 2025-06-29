using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamingPlatform.Models
{
    // Main game entity representing a video game in the platform
    public class Game
    {
        // Primary key
        [Key]
        public int Id { get; set; }

        // Basic game information
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Developer name cannot exceed 100 characters")]
        public string Developer { get; set; } = string.Empty;

        [Column("ReleaseDate")]
        [Display(Name = "Release Date")]
        public DateTime? ReleaseDate { get; set; }

        // Game categorization and rating
        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; } = string.Empty;

        [Range(0, 10, ErrorMessage = "Rating must be between 0 and 10")]
        [Display(Name = "Rating")]
        public decimal Rating { get; set; }

        // Download and system information
        [Column("DownloadLink")]
        [StringLength(500, ErrorMessage = "Download link cannot exceed 500 characters")]
        [Display(Name = "Download Link")]
        public string DownloadLink { get; set; } = string.Empty;

        [Column("SystemRequirements")]
        [StringLength(1000, ErrorMessage = "System requirements cannot exceed 1000 characters")]
        [Display(Name = "System Requirements")]
        public string SystemRequirements { get; set; } = string.Empty;

        // Navigation property for associated media files
        public virtual MediaFile? MediaFile { get; set; }
    }
}