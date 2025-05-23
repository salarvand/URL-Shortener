using System;

namespace URLShortener.Application.Models
{
    public class CreateShortUrlDto
    {
        public string OriginalUrl { get; set; }
        
        public string? CustomShortCode { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
    }
} 