namespace URLShortener.Tests.Models
{
    /// <summary>
    /// Represents the result of a URL security scan for testing purposes
    /// </summary>
    public class UrlScanResult
    {
        /// <summary>
        /// Indicates whether the URL is safe to use
        /// </summary>
        public bool IsSafe { get; set; } = true;

        /// <summary>
        /// Provides a reason why the URL is not safe (if applicable)
        /// </summary>
        public string? Reason { get; set; }
    }
} 