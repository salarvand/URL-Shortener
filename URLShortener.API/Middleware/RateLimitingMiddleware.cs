using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using URLShortener.Application.Interfaces;

namespace URLShortener.API.Middleware
{
    /// <summary>
    /// Middleware for rate limiting requests
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context, IRateLimiter rateLimiter)
        {
            var endpoint = GetEndpointName(context);
            var ipAddress = GetClientIpAddress(context);

            // Skip rate limiting for certain paths
            if (ShouldSkipRateLimiting(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Check if the request is allowed
            var isAllowed = await rateLimiter.IsAllowedAsync(ipAddress, endpoint);
            if (!isAllowed)
            {
                _logger.LogWarning("Rate limit exceeded for IP {IpAddress} on endpoint {Endpoint}", ipAddress, endpoint);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Rate limit exceeded. Please try again later.\"}");
                return;
            }

            // Record the request
            await rateLimiter.RecordRequestAsync(ipAddress, endpoint);

            // Continue processing the request
            await _next(context);
        }

        /// <summary>
        /// Gets the endpoint name from the request path
        /// </summary>
        private string GetEndpointName(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            
            if (path.Contains("/api/shorturl") && context.Request.Method == "POST")
                return "create";
                
            if (path.StartsWith("/s/") || path.Contains("/redirect/"))
                return "redirect";
                
            return "default";
        }

        /// <summary>
        /// Gets the client IP address from the request
        /// </summary>
        private string GetClientIpAddress(HttpContext context)
        {
            // Try to get the IP from X-Forwarded-For header first (for clients behind proxies)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // X-Forwarded-For may contain multiple IPs, take the first one
                var ips = forwardedFor.Split(',');
                return ips[0].Trim();
            }

            // Fall back to the remote IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        /// <summary>
        /// Determines if rate limiting should be skipped for the given path
        /// </summary>
        private bool ShouldSkipRateLimiting(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? "";
            
            // Skip rate limiting for static files and health checks
            return pathValue.StartsWith("/swagger") ||
                   pathValue.StartsWith("/health") ||
                   pathValue.EndsWith(".js") ||
                   pathValue.EndsWith(".css") ||
                   pathValue.EndsWith(".png") ||
                   pathValue.EndsWith(".jpg") ||
                   pathValue.EndsWith(".ico");
        }
    }
} 