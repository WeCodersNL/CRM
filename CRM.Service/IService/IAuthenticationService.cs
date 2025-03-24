using CRM.Model.ApplicationModels;
using CRM.Model.InputModels;

namespace CRM.Service.IService
{
    public interface IAuthenticationService
    {
        Task<ResponseModel<bool>> LoginAsync(ApplicationUserLoginInputModel model);
        Task<ResponseModel<bool>> RegisterAsync(ApplicationUserRegisterInputModel model);
        Task<ResponseModel<bool>> ConfirmEmailAsync(ApplicationUserConfirmEmailInputModel model);
        Task<ResponseModel<bool>> ConfirmEmailVerifyCodeAsync(ApplicationUserConfirmEmailInputModel model);
        Task<bool> ForgotPasswordAsync(ApplicationUserRegisterInputModel model);
        Task<bool> ResetPasswordAsync(ApplicationUserRegisterInputModel model);
        Task<bool> ChangePasswordAsync(ApplicationUserRegisterInputModel model);
        Task<bool> RefreshTokenAsync(ApplicationUserRegisterInputModel model);
    }
}
