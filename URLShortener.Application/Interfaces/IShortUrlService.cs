using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using URLShortener.Application.Models;

namespace URLShortener.Application.Interfaces
{
    public interface IShortUrlService
    {
        Task<ShortUrlDto> CreateShortUrlAsync(CreateShortUrlDto createShortUrlDto);
        Task<ShortUrlDto> GetShortUrlByCodeAsync(string shortCode);
        Task<string> RedirectAndTrackAsync(string shortCode);
        Task<IEnumerable<ShortUrlDto>> GetAllShortUrlsAsync();
        Task<ShortUrlDto> GetShortUrlDetailsByIdAsync(Guid id);
    }
} 