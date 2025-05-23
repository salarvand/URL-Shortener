using System;
using System.Collections.Generic;

namespace URLShortener.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing a valid URL
    /// </summary>
    public class UrlValue : ValueObject
    {
        public string Value { get; }

        private UrlValue(string url)
        {
            Value = url;
        }

        public static UrlValue Create(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be empty", nameof(url));

            // Validate URL format using Uri
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Invalid URL format. URL must be a valid absolute HTTP or HTTPS URL.", nameof(url));
            }

            return new UrlValue(url);
        }

        // Implicit conversion to string for convenience
        public static implicit operator string(UrlValue urlValue) => urlValue.Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
} 