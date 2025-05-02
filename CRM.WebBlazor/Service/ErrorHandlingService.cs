using CRM.Model.ApplicationModels;
using CRM.WebBlazor.LocalizationResource;
using Microsoft.Extensions.Localization;
using System.Net;

namespace CRM.WebBlazor.Service
{
    public class ErrorHandlingService(IStringLocalizer<Resource> localizer) : IErrorHandlingService
    {
        public async Task<ResponseModel<T>> HandleErrorResponse<T>(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ResponseModel<T>>();
                return new ResponseModel<T>
                {
                    IsSuccess = false,
                    Message = GetLocalizedErrorMessage(errorResponse?.Message) ?? localizer["message-unknown-error"]
                };
            }

            return new ResponseModel<T>
            {
                IsSuccess = false,
                Message = localizer["message-unknown-error"]
            };
        }

        public ResponseModel<T> HandleErrorResponse<T>(ResponseModel<T>? response)
        {
            return new ResponseModel<T>
            {
                IsSuccess = false,
                Message = GetLocalizedErrorMessage(response?.Message) ?? localizer["message-unknown-error"]
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
                "Unable to reset password" => "message-unable-reset-password",
                "Password change failed" => "message-password-change-failed",
                "User is inactive" => "message-user-is-inactive",
                _ => "message-unknown-error"
            };

            return localizer[resourceKey];
        }
    }
}
