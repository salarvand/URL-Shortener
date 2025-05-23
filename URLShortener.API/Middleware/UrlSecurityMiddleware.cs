using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using URLShortener.Application.Features.ShortUrls;
using URLShortener.Application.Interfaces;

namespace URLShortener.API.Middleware
{
    /// <summary>
    /// Middleware for scanning URLs for security threats
    /// </summary>
    public class UrlSecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UrlSecurityMiddleware> _logger;

        public UrlSecurityMiddleware(RequestDelegate next, ILogger<UrlSecurityMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context, IUrlScanningService urlScanningService)
        {
            // Only check POST requests to /api/shorturl
            if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/api/shorturl"))
            {
                // Need to enable buffering to read the request body multiple times
                context.Request.EnableBuffering();

                // Read the request body
                var requestBody = await ReadRequestBody(context.Request);
                
                // Try to parse the request body to extract the URL
                if (TryExtractUrl(requestBody, out var url))
                {
                    // Check if the URL is safe
                    var isSafe = await urlScanningService.IsSafeUrlAsync(url);
                    if (!isSafe)
                    {
                        var threatInfo = await urlScanningService.GetThreatInfoAsync(url);
                        _logger.LogWarning("Blocked malicious URL: {Url}, Threat: {Threat}", url, threatInfo);
                        
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        context.Response.ContentType = "application/json";
                        
                        var response = new
                        {
                            error = "The URL was flagged as potentially malicious and has been blocked.",
                            details = threatInfo
                        };
                        
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                        return;
                    }
                }
                
                // Reset the request body position for the next middleware
                context.Request.Body.Position = 0;
            }

            // Continue processing the request
            await _next(context);
        }

        /// <summary>
        /// Reads the request body as a string
        /// </summary>
        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            using var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);
                
            var body = await reader.ReadToEndAsync();
            
            // Reset the request body position for the next middleware
            request.Body.Position = 0;
            
            return body;
        }

        /// <summary>
        /// Tries to extract the URL from the request body
        /// </summary>
        private bool TryExtractUrl(string requestBody, out string url)
        {
            url = null;
            
            try
            {
                // Try to parse the request body as JSON
                var jsonDocument = JsonDocument.Parse(requestBody);
                var root = jsonDocument.RootElement;
                
                // Check if this is a CreateShortUrl command
                if (root.TryGetProperty("originalUrl", out var originalUrlElement))
                {
                    url = originalUrlElement.GetString();
                    return !string.IsNullOrEmpty(url);
                }
                
                return false;
            }
            catch (JsonException)
            {
                // If we can't parse the JSON, we can't extract the URL
                return false;
            }
        }
    }
} 