using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamingPlatform.Models
{
    // Entity for storing media files (images) associated with games
    public class MediaFile
    {
        // Primary key and foreign key reference to the associated game
        [Key]
        [Column("GameID")]
        [Required(ErrorMessage = "Game ID is required")]
        public int GameID { get; set; }

        // URLs to media files stored in Cloudinary
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        [Display(Name = "Banner")]
        public string Banner { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        [Display(Name = "Icon")]
        public string Icon { get; set; } = string.Empty;

        [Column("MediaFile1")]
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        [Display(Name = "First Media")]
        public string FirstMediaFile { get; set; } = string.Empty;

        [Column("MediaFile2")]
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        [Display(Name = "Second Media")]
        public string SecondMediaFile { get; set; } = string.Empty;

        [Column("MediaFile3")]
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        [Display(Name = "Third Media")]
        public string ThirdMediaFile { get; set; } = string.Empty;

        [Column("MediaFile4")]
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        [Display(Name = "Fourth Media")]
        public string FourthMediaFile { get; set; } = string.Empty;

        // Navigation property for the associated game
        [ForeignKey("GameID")]
        public virtual Game Game { get; set; } = null!;
    }
}