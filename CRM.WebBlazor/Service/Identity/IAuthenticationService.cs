using CRM.Model.ApplicationModels;
using CRM.Model.InputModels;
using CRM.Model.ViewModels;

namespace CRM.WebBlazor.Service.Identity
{
    public interface IAuthenticationService
    {
        Task<ResponseModel<ApplicationUserProfileViewModel>> LoginAsync(ApplicationUserLoginInputModel model);
        Task<ResponseModel<bool>> RegisterAsync(ApplicationUserRegisterInputModel model);
        Task<ResponseModel<bool>> ConfirmEmailAsync(ApplicationUserConfirmEmailInputModel model);
        Task<ResponseModel<bool>> VerifyEmailCodeAsync(ApplicationUserConfirmEmailInputModel model);
        Task<ResponseModel<bool>> ForgotPasswordAsync(ApplicationUserForgotPasswordInputModel model);
        Task<ResponseModel<bool>> ResetPasswordAsync(ApplicationUserForgotPasswordInputModel model);
    }
}
