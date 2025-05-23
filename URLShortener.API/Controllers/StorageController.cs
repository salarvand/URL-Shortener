using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using URLShortener.Application.Interfaces;

namespace URLShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly IStorageOptimizer _storageOptimizer;
        private readonly ILogger<StorageController> _logger;

        public StorageController(
            IStorageOptimizer storageOptimizer,
            ILogger<StorageController> logger)
        {
            _storageOptimizer = storageOptimizer ?? throw new ArgumentNullException(nameof(storageOptimizer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets storage statistics
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = await _storageOptimizer.GetStorageStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Manually purges expired URLs
        /// </summary>
        [HttpPost("purge")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PurgeExpiredUrls()
        {
            try
            {
                var purgedCount = await _storageOptimizer.PurgeExpiredUrlsAsync();
                return Ok(new { purgedCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purging expired URLs");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Manually aggregates old click statistics
        /// </summary>
        [HttpPost("aggregate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AggregateClickStatistics([FromQuery] int olderThanDays = 30)
        {
            try
            {
                var olderThan = TimeSpan.FromDays(olderThanDays);
                var aggregatedCount = await _storageOptimizer.AggregateOldClickStatisticsAsync(olderThan);
                return Ok(new { aggregatedCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aggregating click statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Manually compresses old URL data
        /// </summary>
        [HttpPost("compress")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CompressOldUrlData([FromQuery] int olderThanDays = 90)
        {
            try
            {
                var olderThan = TimeSpan.FromDays(olderThanDays);
                var compressedCount = await _storageOptimizer.CompressOldUrlDataAsync(olderThan);
                return Ok(new { compressedCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compressing old URL data");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while processing your request." });
            }
        }
    }
}