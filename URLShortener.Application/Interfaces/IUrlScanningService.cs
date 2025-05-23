using System.Threading.Tasks;

namespace URLShortener.Application.Interfaces
{
    /// <summary>
    /// Service for scanning URLs for security threats
    /// </summary>
    public interface IUrlScanningService
    {
        /// <summary>
        /// Checks if a URL is potentially malicious
        /// </summary>
        /// <param name="url">The URL to check</param>
        /// <returns>True if the URL is safe, false if potentially malicious</returns>
        Task<bool> IsSafeUrlAsync(string url);
        
        /// <summary>
        /// Gets detailed information about why a URL might be unsafe
        /// </summary>
        /// <param name="url">The URL to check</param>
        /// <returns>A string describing potential threats, or null if safe</returns>
        Task<string> GetThreatInfoAsync(string url);
    }
} 