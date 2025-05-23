using Microsoft.EntityFrameworkCore;
using System.Linq;
using URLShortener.Domain;
using URLShortener.Domain.Entities;
using URLShortener.Domain.Events;

namespace URLShortener.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ShortUrl> ShortUrls { get; set; }
        public DbSet<ClickStatistic> ClickStatistics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignore domain events - they are not entities to be stored
            modelBuilder.Ignore<DomainEvent>();
            
            // Configure ShortUrl entity
            modelBuilder.Entity<ShortUrl>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalUrl).IsRequired();
                entity.Property(e => e.ShortCode).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.ShortCode).IsUnique();
                
                // Configure the relationship with ClickStatistics
                entity.HasMany<ClickStatistic>()
                      .WithOne()
                      .HasForeignKey(c => c.ShortUrlId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Ignore domain events collection for persistence
                entity.Ignore(e => e.DomainEvents);
            });
            
            // Configure ClickStatistic entity
            modelBuilder.Entity<ClickStatistic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ShortUrlId).IsRequired();
                entity.Property(e => e.ClickedAt).IsRequired();
                
                // Make optional properties nullable in the database
                entity.Property(e => e.UserAgent).IsRequired(false);
                entity.Property(e => e.IpAddress).IsRequired(false);
                entity.Property(e => e.RefererUrl).IsRequired(false);
                
                // Ignore domain events collection for persistence
                entity.Ignore(e => e.DomainEvents);
            });
        }
    }
} 