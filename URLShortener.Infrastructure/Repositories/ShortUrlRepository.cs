using Microsoft.EntityFrameworkCore;
using URLShortener.Application.Interfaces;
using URLShortener.Domain;
using URLShortener.Infrastructure.Data;

namespace URLShortener.Infrastructure.Repositories
{
    public class ShortUrlRepository : IShortUrlRepository
    {
        private readonly AppDbContext _dbContext;

        public ShortUrlRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<ShortUrl> GetByIdAsync(Guid id)
        {
            return await _dbContext.ShortUrls.FindAsync(id);
        }

        public async Task<ShortUrl> GetByShortCodeAsync(string shortCode)
        {
            return await _dbContext.ShortUrls
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);
        }

        public async Task<IEnumerable<ShortUrl>> GetAllAsync()
        {
            return await _dbContext.ShortUrls.ToListAsync();
        }

        public async Task<ShortUrl> AddAsync(ShortUrl shortUrl)
        {
            await _dbContext.ShortUrls.AddAsync(shortUrl);
            await _dbContext.SaveChangesAsync();
            return shortUrl;
        }

        public async Task UpdateAsync(ShortUrl shortUrl)
        {
            _dbContext.ShortUrls.Update(shortUrl);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ShortCodeExistsAsync(string shortCode)
        {
            return await _dbContext.ShortUrls
                .AnyAsync(u => u.ShortCode == shortCode);
        }
    }
} 