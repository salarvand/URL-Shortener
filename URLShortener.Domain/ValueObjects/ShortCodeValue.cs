using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace URLShortener.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing a valid short code
    /// </summary>
    public class ShortCodeValue : ValueObject
    {
        // Valid characters for short codes 
        private static readonly Regex ValidShortCodePattern = new Regex("^[a-zA-Z0-9_-]{1,20}$");
        
        public string Value { get; }

        private ShortCodeValue(string code)
        {
            Value = code;
        }

        public static ShortCodeValue Create(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Short code cannot be empty", nameof(code));

            if (!ValidShortCodePattern.IsMatch(code))
                throw new ArgumentException("Short code must contain only letters, numbers, underscores, or hyphens, and be 1-20 characters long", nameof(code));

            return new ShortCodeValue(code);
        }

        // Implicit conversion to string for convenience
        public static implicit operator string(ShortCodeValue codeValue) => codeValue.Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
} 