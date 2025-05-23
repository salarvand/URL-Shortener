using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Models;

namespace URLShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShortUrlController : ControllerBase
    {
        private readonly IShortUrlService _shortUrlService;

        public ShortUrlController(IShortUrlService shortUrlService)
        {
            _shortUrlService = shortUrlService ?? throw new ArgumentNullException(nameof(shortUrlService));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ShortUrlDto>> Create([FromBody] CreateShortUrlDto createShortUrlDto)
        {
            try
            {
                var shortUrl = await _shortUrlService.CreateShortUrlAsync(createShortUrlDto);
                return CreatedAtAction(nameof(GetByCode), new { code = shortUrl.ShortCode }, shortUrl);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{code}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShortUrlDto>> GetByCode(string code)
        {
            var shortUrl = await _shortUrlService.GetShortUrlByCodeAsync(code);
            if (shortUrl == null)
                return NotFound();

            return Ok(shortUrl);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ShortUrlDto>>> GetAll()
        {
            var shortUrls = await _shortUrlService.GetAllShortUrlsAsync();
            return Ok(shortUrls);
        }

        [HttpGet("detail/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShortUrlDto>> GetById(Guid id)
        {
            var shortUrl = await _shortUrlService.GetShortUrlDetailsByIdAsync(id);
            if (shortUrl == null)
                return NotFound();

            return Ok(shortUrl);
        }
    }
} 