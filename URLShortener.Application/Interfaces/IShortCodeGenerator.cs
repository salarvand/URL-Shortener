namespace URLShortener.Application.Interfaces
{
    public interface IShortCodeGenerator
    {
        string GenerateShortCode(int length = 6);
    }
} 