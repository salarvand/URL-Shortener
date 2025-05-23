using System;
using URLShortener.Application.Interfaces;

namespace URLShortener.Infrastructure.Services
{
    public class UrlValidator : IUrlValidator
    {
        public bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // Uri class will validate the URL format
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
} 