using CRM.Model.ApplicationModels;
using CRM.Model.InputModels;
using CRM.WebBlazor.LocalizationResource;
using Microsoft.Extensions.Localization;
using System.Net;

namespace CRM.WebBlazor.Service
{
    public class AuthenticationService(HttpClient http, IStringLocalizer<Resource> localizer) : IAuthenticationService
    {
        public async Task<ResponseModel<bool>> ConfirmEmailAsync(ApplicationUserConfirmEmailInputModel model)
        { 
            var response = await http.PostAsJsonAsync("Identity/Authentication/confirm-email", model);

            if (response.IsSuccessStatusCode)
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = true
                };
            }

            return await HandleErrorResponse(response);
        }

        public async Task<ResponseModel<bool>> VerifyEmailCodeAsync(ApplicationUserConfirmEmailInputModel model)
        {
            var response = await http.PostAsJsonAsync("Identity/Authentication/confirm-email-verify-code", model);

            if (response.IsSuccessStatusCode)
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = true
                };
            }

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
                "User not found" => "message-user-not-found",
                "Email already confirmed" => "message-email-already-confirmed",
                "Failed to save verification code" => "message-failed-save-verification-code",
                "Email confirmation failed" => "message-email-confirmation-failed",
                "Invalid confirmation code" => "message-invalid-confirmation-code",
                _ => "message-unknown-error"
            };

            return localizer[resourceKey];
        }
    }
}
