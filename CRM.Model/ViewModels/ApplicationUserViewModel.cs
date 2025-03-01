using CRM.Model.Enums;

namespace CRM.Model.ViewModels
{
    public class ApplicationUserViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public short? VerificationCode { get; set; }
        public string? ImageName { get; set; }
        public bool? Activity { get; set; }
    }
}
