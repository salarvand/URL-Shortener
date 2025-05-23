using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using URLShortener.Application.Interfaces;

namespace URLShortener.Application.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<Dictionary<string, string[]>> ValidateAsync<T>(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var validator = _serviceProvider.GetService(typeof(IValidator<T>)) as IValidator<T>;
            
            if (validator == null)
                return new Dictionary<string, string[]>();

            var validationResult = await validator.ValidateAsync(instance);

            if (validationResult.IsValid)
                return new Dictionary<string, string[]>();

            return validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray()
                );
        }

        public async Task ValidateAndThrowAsync<T>(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var validator = _serviceProvider.GetService(typeof(IValidator<T>)) as IValidator<T>;
            
            if (validator == null)
                return;

            await validator.ValidateAndThrowAsync(instance);
        }
    }
} 