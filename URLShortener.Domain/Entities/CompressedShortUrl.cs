using System;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace URLShortener.Domain.Entities
{
    /// <summary>
    /// Entity representing a compressed version of a ShortUrl for long-term storage
    /// </summary>
    public class CompressedShortUrl : Entity
    {
        public Guid OriginalId { get; private set; }
        public string ShortCode { get; private set; }
        public byte[] CompressedData { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public int TotalClicks { get; private set; }
        public DateTime CompressedAt { get; private set; }

        protected CompressedShortUrl() 
        {
            ShortCode = string.Empty;
            CompressedData = Array.Empty<byte>();
        }

        public static CompressedShortUrl Create(ShortUrl shortUrl)
        {
            if (shortUrl == null)
                throw new ArgumentNullException(nameof(shortUrl));

            // Serialize the original URL to a compressed byte array
            var compressedData = CompressUrl(shortUrl.OriginalUrl);

            var compressedUrl = new CompressedShortUrl
            {
                Id = Guid.NewGuid(),
                OriginalId = shortUrl.Id,
                ShortCode = shortUrl.ShortCode,
                CompressedData = compressedData,
                CreatedAt = shortUrl.CreatedAt,
                ExpiresAt = shortUrl.ExpiresAt,
                TotalClicks = shortUrl.ClickCount,
                CompressedAt = DateTime.UtcNow
            };
            
            return compressedUrl;
        }

        public string GetOriginalUrl()
        {
            return DecompressUrl(CompressedData);
        }

        private static byte[] CompressUrl(string url)
        {
            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                using var writer = new StreamWriter(gzipStream);
                writer.Write(url);
            }
            
            return memoryStream.ToArray();
        }

        private static string DecompressUrl(byte[] compressedData)
        {
            using var memoryStream = new MemoryStream(compressedData);
            using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzipStream, Encoding.UTF8);
            
            return reader.ReadToEnd();
        }
    }
} 