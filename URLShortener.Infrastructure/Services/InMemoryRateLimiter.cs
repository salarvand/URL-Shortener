using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using URLShortener.Application.Interfaces;

namespace URLShortener.Infrastructure.Services
{
    /// <summary>
    /// In-memory implementation of rate limiting
    /// </summary>
    public class InMemoryRateLimiter : IRateLimiter
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<InMemoryRateLimiter> _logger;

        // Default rate limits
        private readonly ConcurrentDictionary<string, RateLimitRule> _rateLimitRules;

        public InMemoryRateLimiter(IMemoryCache cache, ILogger<InMemoryRateLimiter> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Initialize rate limit rules
            _rateLimitRules = new ConcurrentDictionary<string, RateLimitRule>();
            
            // Default rules
            _rateLimitRules["create"] = new RateLimitRule { MaxRequests = 10, TimeWindowInSeconds = 60 }; // 10 requests per minute
            _rateLimitRules["redirect"] = new RateLimitRule { MaxRequests = 60, TimeWindowInSeconds = 60 }; // 60 requests per minute
            _rateLimitRules["default"] = new RateLimitRule { MaxRequests = 30, TimeWindowInSeconds = 60 }; // 30 requests per minute
        }

        /// <summary>
        /// Checks if the request from the given IP should be allowed based on rate limits
        /// </summary>
        public Task<bool> IsAllowedAsync(string ipAddress, string endpoint)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                return Task.FromResult(false);
            }

            var key = GetCacheKey(ipAddress, endpoint);
            var requestCount = _cache.GetOrCreate(key, entry =>
            {
                var rule = GetRuleForEndpoint(endpoint);
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(rule.TimeWindowInSeconds);
                return 0;
            });

            var rule = GetRuleForEndpoint(endpoint);
            var isAllowed = requestCount < rule.MaxRequests;

            if (!isAllowed)
            {
                _logger.LogWarning("Rate limit exceeded for IP {IpAddress} on endpoint {Endpoint}", ipAddress, endpoint);
            }

            return Task.FromResult(isAllowed);
        }

        /// <summary>
        /// Records a request from the given IP to the specified endpoint
        /// </summary>
        public Task RecordRequestAsync(string ipAddress, string endpoint)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                return Task.CompletedTask;
            }

            var key = GetCacheKey(ipAddress, endpoint);
            _cache.TryGetValue(key, out int currentCount);
            
            var rule = GetRuleForEndpoint(endpoint);
            _cache.Set(key, currentCount + 1, TimeSpan.FromSeconds(rule.TimeWindowInSeconds));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the cache key for the given IP and endpoint
        /// </summary>
        private string GetCacheKey(string ipAddress, string endpoint)
        {
            return $"rate_limit:{ipAddress}:{endpoint}";
        }

        /// <summary>
        /// Gets the rate limit rule for the given endpoint
        /// </summary>
        private RateLimitRule GetRuleForEndpoint(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint) || !_rateLimitRules.TryGetValue(endpoint.ToLowerInvariant(), out var rule))
            {
                return _rateLimitRules["default"];
            }
            return rule;
        }

        /// <summary>
        /// Represents a rate limit rule
        /// </summary>
        private class RateLimitRule
        {
            public int MaxRequests { get; set; }
            public int TimeWindowInSeconds { get; set; }
        }
    }
} 