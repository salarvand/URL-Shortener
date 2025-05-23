using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using URLShortener.Application.Interfaces;

namespace URLShortener.API.Controllers
{
    [ApiController]
    [Route("s")]
    public class RedirectController : ControllerBase
    {
        private readonly IShortUrlService _shortUrlService;

        public RedirectController(IShortUrlService shortUrlService)
        {
            _shortUrlService = shortUrlService;
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> Redirect(string code)
        {
            var originalUrl = await _shortUrlService.RedirectAndTrackAsync(code);
            if (string.IsNullOrEmpty(originalUrl))
            {
                return NotFound();
            }

            return Redirect(originalUrl);
        }
    }
} 