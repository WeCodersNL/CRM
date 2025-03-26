using System.ComponentModel.DataAnnotations;

namespace CRM.Model.InputModels
{
    public class ApplicationUserLoginInputModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
