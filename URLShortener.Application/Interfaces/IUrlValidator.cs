namespace URLShortener.Application.Interfaces
{
    public interface IUrlValidator
    {
        bool IsValidUrl(string url);
    }
} 