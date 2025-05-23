using System;
using System.Linq;
using URLShortener.Application.Interfaces;

namespace URLShortener.Infrastructure.Services
{
    public class ShortCodeGenerator : IShortCodeGenerator
    {
        private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private readonly Random _random;

        public ShortCodeGenerator()
        {
            _random = new Random();
        }

        public string GenerateShortCode(int length = 6)
        {
            if (length <= 0)
                throw new ArgumentException("Length must be positive", nameof(length));

            return new string(Enumerable.Repeat(AllowedChars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
} 