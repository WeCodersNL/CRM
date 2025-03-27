using CRM.Model.ApplicationModels;
using CRM.Model.InputModels;
using CRM.WebBlazor.LocalizationResource;
using Microsoft.Extensions.Localization;
using System.Net;

namespace CRM.WebBlazor.Service
{
    public class AuthenticationService(HttpClient http, IStringLocalizer<Resource> localizer) : IAuthenticationService
    {
        public async Task<ResponseModel<bool>> LoginAsync(ApplicationUserLoginInputModel model)
        { 
            var response = await http.PostAsJsonAsync("Identity/Authentication/login", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await HandleErrorResponse(response);
        }
        
        public async Task<ResponseModel<bool>> RegisterAsync(ApplicationUserRegisterInputModel model)
        { 
            var response = await http.PostAsJsonAsync("Identity/Authentication/register", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await HandleErrorResponse(response);
        }
        
        public async Task<ResponseModel<bool>> ConfirmEmailAsync(ApplicationUserConfirmEmailInputModel model)
        { 
            var response = await http.PostAsJsonAsync("Identity/Authentication/confirm-email", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await HandleErrorResponse(response);
        }

        public async Task<ResponseModel<bool>> VerifyEmailCodeAsync(ApplicationUserConfirmEmailInputModel model)
        {
            var response = await http.PostAsJsonAsync("Identity/Authentication/confirm-email-verify-code", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await HandleErrorResponse(response);
        }

        public async Task<ResponseModel<bool>> ForgotPasswordAsync(ApplicationUserForgotPasswordInputModel model)
        {
            var response = await http.PostAsJsonAsync("Identity/Authentication/forgot-password", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await HandleErrorResponse(response);
        }

        public async Task<ResponseModel<bool>> ChangePasswordAsync(ApplicationUserForgotPasswordInputModel model)
        {
            var response = await http.PostAsJsonAsync("Identity/Authentication/change-password", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool>{ IsSuccess = true };

            return await HandleErrorResponse(response);
        }

        private async Task<ResponseModel<bool>> HandleErrorResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ResponseModel<bool>>();
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Message = GetLocalizedErrorMessage(errorResponse?.Message) ?? localizer["message-unknown-error"]
                };
            }

            return new ResponseModel<bool>
            {
                IsSuccess = false,
                Message = localizer["message-unknown-error"]
            };
        }

        private string GetLocalizedErrorMessage(string? message)
        {
            var resourceKey = message switch
            {
                "User is locked out." => "message-user-locked-out",
                "Login is not allowed." => "message-login-not-allowed",
                "Two-factor authentication is required." => "message-two-factor-required",
                "Invalid login attempt." => "message-invalid-login",
                "DuplicateUserName" => "message-email-exists",
                "User not found" => "message-user-not-found",
                "Email already confirmed" => "message-email-already-confirmed",
                "Failed to save verification code" => "message-failed-save-verification-code",
                "Email confirmation failed" => "message-email-confirmation-failed",
                "Invalid confirmation code" => "message-invalid-confirmation-code",
                "Password change failed" => "message-password-change-failed",
                _ => "message-unknown-error"
            };

            return localizer[resourceKey];
        }
    }
}
