using System;
using System.Text;
using System.Threading;
using URLShortener.Application.Interfaces;

namespace URLShortener.Infrastructure.Services
{
    /// <summary>
    /// An efficient short code generator that uses Base62 encoding
    /// combined with a distributed ID generation strategy.
    /// </summary>
    public class Base62ShortCodeGenerator : IShortCodeGenerator
    {
        // Base62 character set (0-9, a-z, A-Z)
        private const string Base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        
        // Epoch start time (Jan 1, 2023) - helps keep IDs smaller
        private static readonly DateTimeOffset Epoch = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        // Thread-safe counter for the same millisecond
        private static long _lastTimestamp = -1L;
        private static int _sequence = 0;
        private static readonly object _lock = new object();

        /// <summary>
        /// Generates a short code using an efficient algorithm:
        /// 1. Uses timestamp for global uniqueness
        /// 2. Uses counter for collisions in the same millisecond
        /// 3. Encodes result in Base62 for shorter, URL-safe codes
        /// </summary>
        /// <param name="length">Minimum length of the code (will be padded if needed)</param>
        /// <returns>A unique short code</returns>
        public string GenerateShortCode(int length = 6)
        {
            if (length <= 0)
                throw new ArgumentException("Length must be positive", nameof(length));

            // Generate a unique ID based on timestamp and sequence
            long id = GetNextId();
            
            // Convert to Base62
            string code = ToBase62(id);
            
            // Pad the code if it's shorter than the requested length
            if (code.Length < length)
                code = code.PadLeft(length, '0');
                
            return code;
        }

        /// <summary>
        /// Generates a unique ID based on:
        /// - Current timestamp (milliseconds since epoch)
        /// - Sequence number (for multiple IDs in the same millisecond)
        /// </summary>
        private long GetNextId()
        {
            lock (_lock)
            {
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - Epoch.ToUnixTimeMilliseconds();
                
                // If current timestamp is less than last timestamp, there's a clock drift
                if (timestamp < _lastTimestamp)
                {
                    timestamp = _lastTimestamp;
                }
                
                // If we're in the same millisecond as the last ID generation
                if (timestamp == _lastTimestamp)
                {
                    // Increment sequence number (0-4095)
                    _sequence = (_sequence + 1) & 0xFFF;
                    
                    // If we've used all sequence numbers in this millisecond, wait for next millisecond
                    if (_sequence == 0)
                    {
                        timestamp = WaitNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    // New millisecond, reset sequence
                    _sequence = 0;
                }
                
                _lastTimestamp = timestamp;
                
                // Combine timestamp and sequence into a single ID
                // Cast sequence to uint to prevent sign extension when using bitwise-or
                return (timestamp << 12) | (uint)_sequence;
            }
        }

        /// <summary>
        /// Waits until the next millisecond if we've exhausted the
        /// sequence numbers for the current millisecond
        /// </summary>
        private long WaitNextMillis(long lastTimestamp)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - Epoch.ToUnixTimeMilliseconds();
            while (timestamp <= lastTimestamp)
            {
                Thread.Sleep(1); // Reduce CPU usage while waiting
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - Epoch.ToUnixTimeMilliseconds();
            }
            return timestamp;
        }

        /// <summary>
        /// Converts a decimal number to Base62 representation
        /// </summary>
        private string ToBase62(long number)
        {
            if (number == 0)
                return Base62Chars[0].ToString();
                
            StringBuilder sb = new StringBuilder();
            
            while (number > 0)
            {
                sb.Insert(0, Base62Chars[(int)(number % 62)]);
                number /= 62;
            }
            
            return sb.ToString();
        }
    }
} 