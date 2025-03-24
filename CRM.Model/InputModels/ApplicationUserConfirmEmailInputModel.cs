using System.ComponentModel.DataAnnotations;

namespace CRM.Model.InputModels
{
    public class ApplicationUserConfirmEmailInputModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        public string FullName { get; set; } = "CRM User";
        public required string EmailTemplate { get; set; }
        public string? Code { get; set; }
    }
}
