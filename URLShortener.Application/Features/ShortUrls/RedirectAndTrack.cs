using System.Threading;
using System.Threading.Tasks;
using MediatR;
using URLShortener.Application.Interfaces;

namespace URLShortener.Application.Features.ShortUrls
{
    public static class RedirectAndTrack
    {
        // Command
        public class Command : IRequest<string>
        {
            public string ShortCode { get; set; }
            public string? UserAgent { get; set; }
            public string? IpAddress { get; set; }
            public string? RefererUrl { get; set; }
        }

        // Handler
        public class Handler : IRequestHandler<Command, string>
        {
            private readonly IShortUrlRepository _shortUrlRepository;

            public Handler(IShortUrlRepository shortUrlRepository)
            {
                _shortUrlRepository = shortUrlRepository;
            }

            public async Task<string> Handle(Command request, CancellationToken cancellationToken)
            {
                var shortUrl = await _shortUrlRepository.GetByShortCodeAsync(request.ShortCode);
                
                if (shortUrl == null || !shortUrl.IsActive || shortUrl.IsExpired())
                    return null;

                // Record click with optional tracking data
                shortUrl.RecordClick(request.UserAgent, request.IpAddress, request.RefererUrl);
                
                await _shortUrlRepository.UpdateAsync(shortUrl);

                return shortUrl.OriginalUrl;
            }
        }
    }
} 