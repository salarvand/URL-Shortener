using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace URLShortener.API.Middleware
{
    /// <summary>
    /// Middleware for adding security headers to all responses
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers
            AddSecurityHeaders(context);
            
            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }

        /// <summary>
        /// Adds security headers to the response
        /// </summary>
        private void AddSecurityHeaders(HttpContext context)
        {
            // Prevents MIME-sniffing a response from the declared content-type
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            
            // Prevents the browser from rendering the page if it detects a potential XSS attack
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            
            // Prevents the page from being framed (clickjacking protection)
            context.Response.Headers["X-Frame-Options"] = "DENY";
            
            // Enforces secure connections to the server
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            
            // Controls where resources can be loaded from
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; " +
                "script-src 'self'; " +
                "style-src 'self'; " +
                "img-src 'self' data:; " +
                "font-src 'self'; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none';";
            
            // Controls how much referrer information should be included with requests
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            
            // Prevents the browser from downloading files that should be displayed in the browser
            context.Response.Headers["X-Download-Options"] = "noopen";
            
            // Enables the Cross-site scripting (XSS) filter in the browser
            context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
        }
    }
} 