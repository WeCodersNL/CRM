using CRM.Model.InputModels;

namespace CRM.Service.IService
{
    public interface IAuthenticationService
    {
        Task<bool> LoginAsync(ApplicationUserInputModel model);
        Task<bool> RegisterAsync(ApplicationUserInputModel model);
        Task<bool> ForgotPasswordAsync(ApplicationUserInputModel model);
        Task<bool> ResetPasswordAsync(ApplicationUserInputModel model);
        Task<bool> ChangePasswordAsync(ApplicationUserInputModel model);
        Task<bool> RefreshTokenAsync(ApplicationUserInputModel model);
    }
}
