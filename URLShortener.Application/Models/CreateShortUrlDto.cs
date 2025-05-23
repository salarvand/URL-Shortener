using System;
using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.Models
{
    public class CreateShortUrlDto
    {
        [Required(ErrorMessage = "Please enter a URL to shorten")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string OriginalUrl { get; set; }
        
        [RegularExpression(@"^[a-zA-Z0-9_-]{0,20}$", ErrorMessage = "Custom short code must contain only letters, numbers, underscores, or hyphens")]
        public string? CustomShortCode { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
    }
} 