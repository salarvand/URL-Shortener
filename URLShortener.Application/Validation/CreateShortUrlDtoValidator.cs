using FluentValidation;
using System;
using URLShortener.Application.Models;

namespace URLShortener.Application.Validation
{
    public class CreateShortUrlDtoValidator : AbstractValidator<CreateShortUrlDto>
    {
        public CreateShortUrlDtoValidator()
        {
            RuleFor(x => x.OriginalUrl)
                .NotEmpty().WithMessage("Please enter a URL to shorten")
                .Must(BeAValidUrl).WithMessage("Please enter a valid URL");

            RuleFor(x => x.CustomShortCode)
                .Matches(@"^[a-zA-Z0-9_-]{0,20}$")
                .When(x => !string.IsNullOrEmpty(x.CustomShortCode))
                .WithMessage("Custom short code must contain only letters, numbers, underscores, or hyphens and be at most 20 characters long");

            RuleFor(x => x.ExpiresAt)
                .Must(BeInFuture).When(x => x.ExpiresAt.HasValue)
                .WithMessage("Expiration date must be in the future");
        }

        private bool BeAValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private bool BeInFuture(DateTime? date)
        {
            return !date.HasValue || date.Value > DateTime.UtcNow;
        }
    }
} 