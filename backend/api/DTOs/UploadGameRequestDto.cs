using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GamingPlatform.DTOs
{
    public class UploadGameRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [StringLength(1000)]
        public string SystemRequirements { get; set; } = string.Empty;

        [Required]
        public IFormFile Icon { get; set; } = null!;

        public List<IFormFile> GameplayImages { get; set; } = new();

        [Required]
        public IFormFile Archive { get; set; } = null!;
    }
}
