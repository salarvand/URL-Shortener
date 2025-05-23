using System.Threading.Tasks;
using URLShortener.Tests.Models;

namespace URLShortener.Tests.Interfaces
{
    /// <summary>
    /// Test interface for URL scanning service that matches the application layer interface
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