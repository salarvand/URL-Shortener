using FluentValidation;
using System;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Models;

namespace URLShortener.Application.Validation
{
    public class CreateShortUrlDtoValidator : AbstractValidator<CreateShortUrlDto>
    {
        private readonly IUrlValidator _urlValidator;

        public CreateShortUrlDtoValidator(IUrlValidator urlValidator)
        {
            _urlValidator = urlValidator ?? throw new ArgumentNullException(nameof(urlValidator));

            RuleFor(x => x.OriginalUrl)
                .NotEmpty().WithMessage("Please enter a URL to shorten")
                .Must(url => _urlValidator.IsValidUrl(url)).WithMessage("Please enter a valid URL")
                .MaximumLength(2000).WithMessage("URL cannot exceed 2000 characters");

            RuleFor(x => x.CustomShortCode)
                .Matches(@"^[a-zA-Z0-9_-]{1,20}$")
                .When(x => !string.IsNullOrEmpty(x.CustomShortCode))
                .WithMessage("Custom short code must contain only letters, numbers, underscores, or hyphens and be between 1-20 characters");

            RuleFor(x => x.ExpiresAt)
                .Must(BeInFuture).When(x => x.ExpiresAt.HasValue)
                .WithMessage("Expiration date must be in the future");
        }

        private bool BeInFuture(DateTime? date)
        {
            return !date.HasValue || date.Value > DateTime.UtcNow;
        }
    }
} 