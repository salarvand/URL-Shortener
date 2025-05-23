using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Models;
using URLShortener.Domain;

namespace URLShortener.Application.Features.ShortUrls
{
    public static class CreateShortUrl
    {
        // Command
        public class Command : IRequest<ShortUrlDto>
        {
            public string OriginalUrl { get; set; }
            public string? CustomShortCode { get; set; }
            public DateTime? ExpiresAt { get; set; }
        }

        // Handler
        public class Handler : IRequestHandler<Command, ShortUrlDto>
        {
            private readonly IShortUrlRepository _shortUrlRepository;
            private readonly IShortCodeGenerator _shortCodeGenerator;
            private readonly IValidationService _validationService;

            public Handler(
                IShortUrlRepository shortUrlRepository,
                IShortCodeGenerator shortCodeGenerator,
                IValidationService validationService)
            {
                _shortUrlRepository = shortUrlRepository ?? throw new ArgumentNullException(nameof(shortUrlRepository));
                _shortCodeGenerator = shortCodeGenerator ?? throw new ArgumentNullException(nameof(shortCodeGenerator));
                _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            }

            public async Task<ShortUrlDto> Handle(Command request, CancellationToken cancellationToken)
            {
                // Convert command to DTO for validation
                var createShortUrlDto = new CreateShortUrlDto
                {
                    OriginalUrl = request.OriginalUrl,
                    CustomShortCode = request.CustomShortCode,
                    ExpiresAt = request.ExpiresAt
                };

                // Validate using FluentValidation
                await _validationService.ValidateAndThrowAsync(createShortUrlDto);

                string shortCode;

                if (!string.IsNullOrWhiteSpace(request.CustomShortCode))
                {
                    shortCode = request.CustomShortCode;
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
                    request.OriginalUrl,
                    shortCode,
                    request.ExpiresAt
                );

                await _shortUrlRepository.AddAsync(shortUrl);

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
} 