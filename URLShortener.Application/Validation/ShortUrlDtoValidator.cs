using FluentValidation;
using System;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Models;

namespace URLShortener.Application.Validation
{
    public class ShortUrlDtoValidator : AbstractValidator<ShortUrlDto>
    {
        private readonly IUrlValidator _urlValidator;

        public ShortUrlDtoValidator(IUrlValidator urlValidator)
        {
            _urlValidator = urlValidator ?? throw new ArgumentNullException(nameof(urlValidator));
            
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID is required");

            RuleFor(x => x.OriginalUrl)
                .NotEmpty().WithMessage("Original URL is required")
                .Must(url => _urlValidator.IsValidUrl(url)).WithMessage("Original URL must be a valid URL");

            RuleFor(x => x.ShortCode)
                .NotEmpty().WithMessage("Short code is required")
                .Matches(@"^[a-zA-Z0-9_-]{1,20}$").WithMessage("Short code must contain only letters, numbers, underscores, or hyphens");

            RuleFor(x => x.CreatedAt)
                .NotEmpty().WithMessage("Creation date is required");
        }
    }
} 