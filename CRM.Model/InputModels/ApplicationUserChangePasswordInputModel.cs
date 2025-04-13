namespace CRM.Model.InputModels
{
    public class ApplicationUserChangePasswordInputModel
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
