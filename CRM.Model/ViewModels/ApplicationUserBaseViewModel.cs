using CRM.Model.Enums;

namespace CRM.Model.ViewModels
{
    public class ApplicationUserBaseViewModel
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? ImageName { get; set; }
        public bool? IsActive { get; set; }
        public string? ProfileDescription { get; set; }
    }
}
