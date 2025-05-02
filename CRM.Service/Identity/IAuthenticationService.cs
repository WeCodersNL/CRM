using CRM.Model.ApplicationModels;
using CRM.Model.InputModels;
using CRM.Model.ViewModels;

namespace CRM.Service.Identity
{
    public interface IAuthenticationService
    {
        Task<ResponseModel<AuthenticationTokens>> LoginAsync(ApplicationUserLoginInputModel model);
        Task<ResponseModel<bool>> RegisterAsync(ApplicationUserRegisterInputModel model);
        Task<ResponseModel<bool>> ConfirmEmailAsync(ApplicationUserConfirmEmailInputModel model);
        Task<ResponseModel<bool>> ConfirmEmailVerifyCodeAsync(ApplicationUserConfirmEmailInputModel model);
        Task<ResponseModel<bool>> ForgotPasswordAsync(ApplicationUserForgotPasswordInputModel model);
        Task<ResponseModel<bool>> ResetPasswordAsync(ApplicationUserForgotPasswordInputModel model);
        Task<ResponseModel<AuthenticationTokens>> RefreshTokenAsync(AuthenticationTokens model);
    }
}
