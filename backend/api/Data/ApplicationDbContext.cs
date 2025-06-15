using Microsoft.EntityFrameworkCore;
using GamingPlatform.Models;

namespace GamingPlatform.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Database sets for entity tables
        public DbSet<Game> Games { get; set; }
        public DbSet<MediaFile> MediaFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Game entity and its indexes
            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Games");
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Title);
                entity.HasIndex(e => e.Rating);
                entity.HasIndex(e => e.ReleaseDate);
            });

            // Configure MediaFile entity and its relationship with Game
            modelBuilder.Entity<MediaFile>(entity =>
            {
                entity.ToTable("MediaFiles");
                entity.HasOne(m => m.Game)
                      .WithOne(g => g.MediaFile)
                      .HasForeignKey<MediaFile>(m => m.GameID)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}