using Microsoft.EntityFrameworkCore;
using URLShortener.Domain;

namespace URLShortener.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ShortUrl> ShortUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ShortUrl>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalUrl).IsRequired();
                entity.Property(e => e.ShortCode).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.ShortCode).IsUnique();
            });
        }
    }
} 