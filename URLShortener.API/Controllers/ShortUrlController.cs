using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using URLShortener.Application.Exceptions;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Models;

namespace URLShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShortUrlController : ControllerBase
    {
        private readonly IShortUrlService _shortUrlService;
        private readonly ILogger<ShortUrlController> _logger;

        public ShortUrlController(
            IShortUrlService shortUrlService,
            ILogger<ShortUrlController> logger)
        {
            _shortUrlService = shortUrlService ?? throw new ArgumentNullException(nameof(shortUrlService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error: {Message}", ex.Message);
                return BadRequest(new { 
                    message = "Validation failed", 
                    errors = ex.Errors 
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Argument error: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operation error: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating short URL");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while processing your request." });
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