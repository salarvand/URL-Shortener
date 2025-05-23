namespace URLShortener.Tests.Models
{
    /// <summary>
    /// Model to pass information about click events for testing
    /// </summary>
    public class ClickTrackingInfo
    {
        /// <summary>
        /// User agent string from the HTTP request
        /// </summary>
        public string? UserAgent { get; set; }
        
        /// <summary>
        /// IP address of the client
        /// </summary>
        public string? IpAddress { get; set; }
        
        /// <summary>
        /// Referer URL if available
        /// </summary>
        public string? RefererUrl { get; set; }
    }
} 