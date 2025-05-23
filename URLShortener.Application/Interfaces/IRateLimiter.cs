using System.Threading.Tasks;

namespace URLShortener.Application.Interfaces
{
    /// <summary>
    /// Interface for rate limiting service
    /// </summary>
    public interface IRateLimiter
    {
        /// <summary>
        /// Checks if the request from the given IP should be allowed based on rate limits
        /// </summary>
        /// <param name="ipAddress">The IP address of the client</param>
        /// <param name="endpoint">The API endpoint being accessed</param>
        /// <returns>True if the request is allowed, false if rate limited</returns>
        Task<bool> IsAllowedAsync(string ipAddress, string endpoint);
        
        /// <summary>
        /// Records a request from the given IP to the specified endpoint
        /// </summary>
        /// <param name="ipAddress">The IP address of the client</param>
        /// <param name="endpoint">The API endpoint being accessed</param>
        Task RecordRequestAsync(string ipAddress, string endpoint);
    }
} 