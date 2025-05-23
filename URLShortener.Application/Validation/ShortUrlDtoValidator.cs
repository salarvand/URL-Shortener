using FluentValidation;
using System;
using URLShortener.Application.Models;

namespace URLShortener.Application.Validation
{
    public class ShortUrlDtoValidator : AbstractValidator<ShortUrlDto>
    {
        public ShortUrlDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID is required");

            RuleFor(x => x.OriginalUrl)
                .NotEmpty().WithMessage("Original URL is required")
                .Must(BeAValidUrl).WithMessage("Original URL must be a valid URL");

            RuleFor(x => x.ShortCode)
                .NotEmpty().WithMessage("Short code is required")
                .Matches(@"^[a-zA-Z0-9_-]{1,20}$").WithMessage("Short code must contain only letters, numbers, underscores, or hyphens");

            RuleFor(x => x.CreatedAt)
                .NotEmpty().WithMessage("Creation date is required");
        }

        private bool BeAValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
} 