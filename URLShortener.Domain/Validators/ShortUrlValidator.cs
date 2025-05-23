using System;
using System.Text.RegularExpressions;

namespace URLShortener.Domain.Validators
{
    public static class ShortUrlValidator
    {
        private static readonly Regex ShortCodeRegex = new Regex(@"^[a-zA-Z0-9_-]{1,20}$", RegexOptions.Compiled);
        
        public static bool IsValidShortCode(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
                return false;
                
            return ShortCodeRegex.IsMatch(shortCode);
        }
        
        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;
                
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
        
        public static bool IsValidExpirationDate(DateTime? expiresAt)
        {
            return !expiresAt.HasValue || expiresAt.Value > DateTime.UtcNow;
        }
    }
} 