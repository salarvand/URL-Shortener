using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using URLShortener.Application.Interfaces;

namespace URLShortener.Infrastructure.Services
{
    /// <summary>
    /// Service for scanning URLs for security threats
    /// </summary>
    public class UrlScanningService : IUrlScanningService
    {
        private readonly ILogger<UrlScanningService> _logger;
        private readonly HttpClient _httpClient;
        
        // List of suspicious patterns in URLs
        private static readonly List<string> SuspiciousPatterns = new List<string>
        {
            @"(?:\.exe|\.dll|\.bat|\.cmd|\.sh|\.ps1)$", // Executable file extensions
            @"(?:phish|malware|trojan|virus|exploit|hack|attack)", // Suspicious keywords
            @"(?:free.*(?:iphone|money|gift|prize|winner))", // Common scam patterns
            @"(?:password|login|signin|account).*(?:verify|confirm|update|reset)", // Phishing patterns
            @"(?:bit\.ly|tinyurl\.com|goo\.gl|t\.co|is\.gd)\/", // URL shorteners (potentially hiding malicious URLs)
        };
        
        // Compiled regex patterns for better performance
        private static readonly List<Regex> SuspiciousRegexPatterns = SuspiciousPatterns
            .ConvertAll(pattern => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
            
        // Domains known to be associated with phishing or malware
        private static readonly HashSet<string> KnownMaliciousDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "malware-site.com",
            "phishing-example.com",
            "totally-not-a-scam.com",
            "free-v-bucks-generator.com",
            "get-rich-quick-scheme.com"
        };

        public UrlScanningService(ILogger<UrlScanningService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        /// <summary>
        /// Checks if a URL is potentially malicious
        /// </summary>
        public async Task<bool> IsSafeUrlAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            try
            {
                // Check if the URL contains suspicious patterns
                if (ContainsSuspiciousPatterns(url))
                {
                    _logger.LogWarning("URL contains suspicious patterns: {Url}", url);
                    return false;
                }

                // Check if the domain is in our known malicious domains list
                if (IsKnownMaliciousDomain(url))
                {
                    _logger.LogWarning("URL domain is in known malicious domains list: {Url}", url);
                    return false;
                }

                // In a real implementation, we would call an external API for URL scanning
                // For example: Google Safe Browsing, VirusTotal, etc.
                // For this example, we'll just return true for most URLs
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking URL safety: {Url}", url);
                // If we can't check, assume it's unsafe
                return false;
            }
        }

        /// <summary>
        /// Gets detailed information about why a URL might be unsafe
        /// </summary>
        public async Task<string> GetThreatInfoAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return "Empty URL";
            }

            try
            {
                // Check if the URL contains suspicious patterns
                foreach (var pattern in SuspiciousRegexPatterns)
                {
                    if (pattern.IsMatch(url))
                    {
                        return $"URL contains suspicious pattern: {pattern}";
                    }
                }

                // Check if the domain is in our known malicious domains list
                if (IsKnownMaliciousDomain(url))
                {
                    return "URL domain is in known malicious domains list";
                }

                // In a real implementation, we would call an external API for URL scanning
                return null; // No threats found
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting threat info: {Url}", url);
                return "Could not scan URL for threats";
            }
        }

        /// <summary>
        /// Checks if the URL contains any suspicious patterns
        /// </summary>
        private bool ContainsSuspiciousPatterns(string url)
        {
            foreach (var pattern in SuspiciousRegexPatterns)
            {
                if (pattern.IsMatch(url))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the domain of the URL is in our known malicious domains list
        /// </summary>
        private bool IsKnownMaliciousDomain(string url)
        {
            try
            {
                var uri = new Uri(url);
                var domain = uri.Host.ToLowerInvariant();
                
                return KnownMaliciousDomains.Contains(domain);
            }
            catch
            {
                // If we can't parse the URL, assume it's suspicious
                return true;
            }
        }
    }
} 