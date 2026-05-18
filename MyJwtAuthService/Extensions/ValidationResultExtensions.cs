using FluentValidation.Results;

namespace MyJwtAuthService.Extensions
{
    public static class ValidationResultExtensions
    {
        public static IDictionary<string, string[]> GetValidationErrors(this ValidationResult validationResult)
        {
            return validationResult.Errors.GroupBy(x => x.PropertyName).ToDictionary(x => x.Key, x => x.Select(x => x.ErrorMessage).ToArray());
        }
    }
}
