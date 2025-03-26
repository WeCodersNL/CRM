namespace CRM.Model.InputModels
{
    public class ApplicationUserForgotPasswordInputModel : ApplicationUserVerificationBaseInputModel
    {
        public string? Password { get; set; }
    }
}
