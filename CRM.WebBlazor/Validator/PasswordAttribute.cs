using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CRM.WebBlazor.Validator
{
    public partial class PasswordAttribute : ValidationAttribute
    {
        // A regular expression to match passwords with at least 8 characters, one uppercase letter, one number and one symbol
        private static readonly Regex _regex = PasswordRegex();

        // Override the IsValid method to check the value against the regex
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // If the value is null or empty, return success
            // This attribute is not responsible for checking required fields
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            // If the value matches the regex, return success
            if (_regex.IsMatch(value.ToString()!))
            {
                return ValidationResult.Success;
            }

            // Otherwise, return failure with the error message
            return new ValidationResult(ErrorMessage);
        }

        [GeneratedRegex(@"^(?=.*[A-Z])(?=.*\d)(?=.*\W)[A-Za-z\d\W]{8,}$")]
        private static partial Regex PasswordRegex();
    }

}
