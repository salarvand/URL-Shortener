using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using URLShortener.Domain;

namespace URLShortener.Application.Interfaces
{
    public interface IShortUrlRepository
    {
        Task<ShortUrl> GetByIdAsync(Guid id);
        Task<ShortUrl> GetByShortCodeAsync(string shortCode);
        Task<IEnumerable<ShortUrl>> GetAllAsync();
        Task<ShortUrl> AddAsync(ShortUrl shortUrl);
        Task UpdateAsync(ShortUrl shortUrl);
        Task<bool> ShortCodeExistsAsync(string shortCode);
    }
} 