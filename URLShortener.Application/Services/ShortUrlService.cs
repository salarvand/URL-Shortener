using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Models;
using URLShortener.Domain;

namespace URLShortener.Application.Services
{
    public class ShortUrlService : IShortUrlService
    {
        private readonly IShortUrlRepository _shortUrlRepository;
        private readonly IShortCodeGenerator _shortCodeGenerator;
        private readonly IValidationService _validationService;

        public ShortUrlService(
            IShortUrlRepository shortUrlRepository,
            IShortCodeGenerator shortCodeGenerator,
            IValidationService validationService)
        {
            _shortUrlRepository = shortUrlRepository ?? throw new ArgumentNullException(nameof(shortUrlRepository));
            _shortCodeGenerator = shortCodeGenerator ?? throw new ArgumentNullException(nameof(shortCodeGenerator));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        public async Task<ShortUrlDto> CreateShortUrlAsync(CreateShortUrlDto createShortUrlDto)
        {
            if (createShortUrlDto == null)
                throw new ArgumentNullException(nameof(createShortUrlDto));

            // Validate using FluentValidation
            await _validationService.ValidateAndThrowAsync(createShortUrlDto);

            string shortCode;

            if (!string.IsNullOrWhiteSpace(createShortUrlDto.CustomShortCode))
            {
                shortCode = createShortUrlDto.CustomShortCode;
                var exists = await _shortUrlRepository.ShortCodeExistsAsync(shortCode);
                if (exists)
                    throw new InvalidOperationException($"Short code '{shortCode}' is already in use");
            }
            else
            {
                // Generate a unique short code
                do
                {
                    shortCode = _shortCodeGenerator.GenerateShortCode();
                } while (await _shortUrlRepository.ShortCodeExistsAsync(shortCode));
            }

            // Use domain factory method to create the entity
            var shortUrl = ShortUrl.Create(
                createShortUrlDto.OriginalUrl,
                shortCode,
                createShortUrlDto.ExpiresAt
            );

            await _shortUrlRepository.AddAsync(shortUrl);

            return MapToDto(shortUrl);
        }

        public async Task<ShortUrlDto> GetShortUrlByCodeAsync(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
                throw new ArgumentException("Short code cannot be empty", nameof(shortCode));

            var shortUrl = await _shortUrlRepository.GetByShortCodeAsync(shortCode);
            if (shortUrl == null)
                return null;

            return MapToDto(shortUrl);
        }

        public async Task<string> RedirectAndTrackAsync(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
                throw new ArgumentException("Short code cannot be empty", nameof(shortCode));

            var shortUrl = await _shortUrlRepository.GetByShortCodeAsync(shortCode);
            if (shortUrl == null)
                return null;

            if (!shortUrl.IsActive || shortUrl.IsExpired())
                return null;

            shortUrl.IncrementClickCount();
            await _shortUrlRepository.UpdateAsync(shortUrl);

            return shortUrl.OriginalUrl;
        }

        public async Task<IEnumerable<ShortUrlDto>> GetAllShortUrlsAsync()
        {
            var shortUrls = await _shortUrlRepository.GetAllAsync();
            return shortUrls.Select(MapToDto);
        }

        public async Task<ShortUrlDto> GetShortUrlDetailsByIdAsync(Guid id)
        {
            var shortUrl = await _shortUrlRepository.GetByIdAsync(id);
            if (shortUrl == null)
                return null;

            return MapToDto(shortUrl);
        }

        private ShortUrlDto MapToDto(ShortUrl shortUrl)
        {
            if (shortUrl == null)
                return null;

            return new ShortUrlDto
            {
                Id = shortUrl.Id,
                OriginalUrl = shortUrl.OriginalUrl,
                ShortCode = shortUrl.ShortCode,
                ShortUrl = $"/s/{shortUrl.ShortCode}",
                CreatedAt = shortUrl.CreatedAt,
                ExpiresAt = shortUrl.ExpiresAt,
                ClickCount = shortUrl.ClickCount,
                IsActive = shortUrl.IsActive,
                IsExpired = shortUrl.IsExpired()
            };
        }
    }
} 