using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CRM.WebBlazor.Validator
{
    public partial class EmailOnlyAttribute : ValidationAttribute
    {
        private static readonly Regex _regex = EmailOnlyRegex();

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            if (_regex.IsMatch(value.ToString()!))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }

        [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        private static partial Regex EmailOnlyRegex();
    }
}
