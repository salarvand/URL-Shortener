using Microsoft.AspNetCore.Builder;

namespace URLShortener.API.Middleware
{
    /// <summary>
    /// Extension methods for middleware registration
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds rate limiting middleware to the request pipeline
        /// </summary>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
        
        /// <summary>
        /// Adds URL security middleware to the request pipeline
        /// </summary>
        public static IApplicationBuilder UseUrlSecurity(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UrlSecurityMiddleware>();
        }
        
        /// <summary>
        /// Adds security headers middleware to the request pipeline
        /// </summary>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}