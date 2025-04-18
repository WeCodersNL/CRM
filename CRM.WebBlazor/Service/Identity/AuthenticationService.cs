using CRM.Model.ApplicationModels;
using CRM.Model.InputModels;
using CRM.Model.ViewModels;
using Microsoft.JSInterop;
using System.Text.Json;

namespace CRM.WebBlazor.Service.Identity
{
    public class AuthenticationService(HttpClient http, IErrorHandlingService errorHandlingService, IJSRuntime jsRuntime) : IAuthenticationService
    {
        public async Task<ResponseModel<ApplicationUserProfileViewModel>> LoginAsync(ApplicationUserLoginInputModel model)
        {
            var response = await http.PostAsJsonAsync("Identity/Authentication/login", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ResponseModel<ApplicationUserProfileViewModel>>();
                if (result != null && result.IsSuccess && result.Data != null)
                {
                    var userProfile = result.Data;
                    await jsRuntime.InvokeVoidAsync("localStorage.setItem", "user", JsonSerializer.Serialize(userProfile));
                    return new ResponseModel<ApplicationUserProfileViewModel> { IsSuccess = true };
                }
            }

            return await errorHandlingService.HandleErrorResponse<ApplicationUserProfileViewModel>(response);
        }

        public async Task<ResponseModel<bool>> RegisterAsync(ApplicationUserRegisterInputModel model)
        {
            var response = await http.PostAsJsonAsync("Identity/Authentication/register", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await errorHandlingService.HandleErrorResponse<bool>(response);
        }

        public async Task<ResponseModel<bool>> ConfirmEmailAsync(ApplicationUserConfirmEmailInputModel model)
        {
            var response = await http.PostAsJsonAsync("Identity/Authentication/confirm-email", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await errorHandlingService.HandleErrorResponse<bool>(response);
        }

        public async Task<ResponseModel<bool>> VerifyEmailCodeAsync(ApplicationUserConfirmEmailInputModel model)
        {
            var response = await http.PostAsJsonAsync("Identity/Authentication/confirm-email-verify-code", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await errorHandlingService.HandleErrorResponse<bool>(response);
        }

        public async Task<ResponseModel<bool>> ForgotPasswordAsync(ApplicationUserForgotPasswordInputModel model)
        {
            var response = await http.PostAsJsonAsync("Identity/Authentication/forgot-password", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await errorHandlingService.HandleErrorResponse<bool>(response);
        }

        public async Task<ResponseModel<bool>> ResetPasswordAsync(ApplicationUserForgotPasswordInputModel model)
        {
            var response = await http.PostAsJsonAsync("Identity/Authentication/reset-password", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await errorHandlingService.HandleErrorResponse<bool>(response);
        }


    }
}
