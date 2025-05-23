using System.Threading;
using System.Threading.Tasks;
using MediatR;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Models;

namespace URLShortener.Application.Features.ShortUrls
{
    public static class GetShortUrlByCode
    {
        // Query
        public class Query : IRequest<ShortUrlDto>
        {
            public string ShortCode { get; set; }
        }

        // Handler
        public class Handler : IRequestHandler<Query, ShortUrlDto>
        {
            private readonly IShortUrlRepository _shortUrlRepository;

            public Handler(IShortUrlRepository shortUrlRepository)
            {
                _shortUrlRepository = shortUrlRepository;
            }

            public async Task<ShortUrlDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var shortUrl = await _shortUrlRepository.GetByShortCodeAsync(request.ShortCode);
                
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
} 