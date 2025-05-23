using System;
using System.Collections.Generic;
using URLShortener.Domain;
using URLShortener.Domain.Entities;

namespace URLShortener.Tests.Common
{
    public static class TestDataFactory
    {
        public static ShortUrl CreateValidShortUrl(
            string? originalUrl = null,
            string? shortCode = null,
            DateTime? createdAt = null,
            int clickCount = 0,
            bool isActive = true,
            DateTime? expiresAt = null)
        {
            return ShortUrl.Reconstitute(
                Guid.NewGuid(),
                originalUrl ?? "https://example.com",
                shortCode ?? "abc123",
                createdAt ?? DateTime.UtcNow,
                clickCount,
                isActive,
                expiresAt
            );
        }

        public static List<ShortUrl> CreateMultipleShortUrls(int count)
        {
            var shortUrls = new List<ShortUrl>();
            for (int i = 0; i < count; i++)
            {
                shortUrls.Add(CreateValidShortUrl(
                    originalUrl: $"https://example.com/page{i}",
                    shortCode: $"code{i}",
                    clickCount: i
                ));
            }
            return shortUrls;
        }

        public static ClickStatistic CreateClickStatistic(
            Guid? shortUrlId = null,
            string? userAgent = null,
            string? ipAddress = null,
            string? refererUrl = null)
        {
            var statistic = ClickStatistic.Create(
                shortUrlId ?? Guid.NewGuid(),
                userAgent,
                ipAddress,
                refererUrl
            );
            
            return statistic;
        }

        public static List<ClickStatistic> CreateMultipleClickStatistics(int count, Guid shortUrlId)
        {
            var clickStats = new List<ClickStatistic>();
            
            for (int i = 0; i < count; i++)
            {
                clickStats.Add(CreateClickStatistic(
                    shortUrlId: shortUrlId,
                    userAgent: i % 3 == 0 ? "Chrome" : i % 3 == 1 ? "Firefox" : "Safari",
                    ipAddress: $"192.168.1.{i % 255}",
                    refererUrl: i % 2 == 0 ? "https://google.com" : "https://bing.com"
                ));
            }
            
            return clickStats;
        }

        public static AggregatedClickStatistic CreateAggregatedClickStatistic(
            Guid? shortUrlId = null,
            DateTime? periodStart = null,
            DateTime? periodEnd = null,
            int clickCount = 10)
        {
            var start = periodStart ?? DateTime.UtcNow.AddDays(-30);
            var end = periodEnd ?? DateTime.UtcNow;
            
            return AggregatedClickStatistic.Create(
                shortUrlId ?? Guid.NewGuid(),
                start,
                end,
                clickCount,
                "{\"Chrome\":5,\"Firefox\":3,\"Safari\":2}",
                "{\"192.168.1.1\":3,\"192.168.1.2\":7}",
                "{\"google.com\":6,\"bing.com\":4}"
            );
        }

        public static CompressedShortUrl CreateCompressedShortUrl(ShortUrl shortUrl)
        {
            return CompressedShortUrl.Create(shortUrl);
        }
    }
} 