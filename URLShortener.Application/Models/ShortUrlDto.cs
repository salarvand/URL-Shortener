using System;

namespace URLShortener.Application.Models
{
    public class ShortUrlDto
    {
        public Guid Id { get; set; }
        public string OriginalUrl { get; set; }
        public string ShortCode { get; set; }
        public string ShortUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int ClickCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
    }
} 