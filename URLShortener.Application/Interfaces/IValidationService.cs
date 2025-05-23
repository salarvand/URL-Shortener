using System.Collections.Generic;
using System.Threading.Tasks;

namespace URLShortener.Application.Interfaces
{
    /// <summary>
    /// Generic validation service interface for domain-driven validation
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validates an object and returns validation results
        /// </summary>
        /// <typeparam name="T">Type of object to validate</typeparam>
        /// <param name="instance">Object instance to validate</param>
        /// <returns>Dictionary of property names and error messages</returns>
        Task<Dictionary<string, string[]>> ValidateAsync<T>(T instance);
        
        /// <summary>
        /// Validates an object and throws an exception if validation fails
        /// </summary>
        /// <typeparam name="T">Type of object to validate</typeparam>
        /// <param name="instance">Object instance to validate</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        Task ValidateAndThrowAsync<T>(T instance);
    }
} 