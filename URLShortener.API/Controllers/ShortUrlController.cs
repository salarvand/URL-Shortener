using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using URLShortener.Application.Exceptions;
using URLShortener.Application.Features.ShortUrls;
using URLShortener.Application.Models;

namespace URLShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShortUrlController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ShortUrlController> _logger;

        public ShortUrlController(
            IMediator mediator,
            ILogger<ShortUrlController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ShortUrlDto>> Create([FromBody] CreateShortUrl.Command command)
        {
            try
            {
                var shortUrl = await _mediator.Send(command);
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
            var query = new GetShortUrlByCode.Query { ShortCode = code };
            var shortUrl = await _mediator.Send(query);
            
            if (shortUrl == null)
                return NotFound();

            return Ok(shortUrl);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ShortUrlDto>>> GetAll()
        {
            var query = new GetAllShortUrls.Query();
            var shortUrls = await _mediator.Send(query);
            
            return Ok(shortUrls);
        }

        [HttpGet("detail/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShortUrlDto>> GetById(Guid id)
        {
            // This endpoint could be implemented with a new query
            // For now, we'll return NotImplemented
            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        [HttpGet("redirect/{code}")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Redirect(string code)
        {
            var command = new RedirectAndTrack.Command 
            { 
                ShortCode = code,
                UserAgent = Request.Headers.UserAgent.ToString(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                RefererUrl = Request.Headers.Referer.ToString()
            };
            
            var originalUrl = await _mediator.Send(command);
            
            if (string.IsNullOrEmpty(originalUrl))
                return NotFound();
                
            return base.Redirect(originalUrl);
        }
    }
} 