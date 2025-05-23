using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Models;

namespace URLShortener.Application.Features.ShortUrls
{
    public static class GetAllShortUrls
    {
        // Query
        public class Query : IRequest<List<ShortUrlDto>>
        {
        }

        // Handler
        public class Handler : IRequestHandler<Query, List<ShortUrlDto>>
        {
            private readonly IShortUrlRepository _shortUrlRepository;

            public Handler(IShortUrlRepository shortUrlRepository)
            {
                _shortUrlRepository = shortUrlRepository;
            }

            public async Task<List<ShortUrlDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var shortUrls = await _shortUrlRepository.GetAllAsync();
                
                return shortUrls.Select(shortUrl => new ShortUrlDto
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
                }).ToList();
            }
        }
    }
} 