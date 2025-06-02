using CRM.Model.ApplicationModels;
using CRM.Model.InputModels;
using Microsoft.JSInterop;

namespace CRM.WebBlazor.Service.Identity
{
    public class AuthenticationService(
        IHttpClientFactory httpClientFactory
        , IErrorHandlingService errorHandlingService
        , IJSRuntime jsRuntime
        , TokenStore tokenStore
        ) : IAuthenticationService
    {
        private readonly HttpClient httpClient = httpClientFactory.CreateClient("DefaultClient");
        public async Task<ResponseModel<AuthenticationTokens>> LoginAsync(ApplicationUserLoginInputModel model)
        {
            var response = await httpClient.PostAsJsonAsync("Identity/Authentication/login", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ResponseModel<AuthenticationTokens>>();
                if (result != null && result.IsSuccess && result.Data != null)
                {
                    tokenStore.AccessToken = result.Data.AccessToken;
                    tokenStore.RefreshToken = result.Data.RefreshToken;
                    tokenStore.IsAuthenticated = true;
                    LogsEnricher.CurrentUserEmail = model.Email;
                    await tokenStore.SaveToStorageAsync(jsRuntime);
                    return new ResponseModel<AuthenticationTokens> { IsSuccess = true };
                }
            }

            return await errorHandlingService.HandleErrorResponse<AuthenticationTokens>(response);
        }

        public async Task<ResponseModel<bool>> RegisterAsync(ApplicationUserRegisterInputModel model)
        {
            var response = await httpClient.PostAsJsonAsync("Identity/Authentication/register", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await errorHandlingService.HandleErrorResponse<bool>(response);
        }

        public async Task<ResponseModel<bool>> ConfirmEmailAsync(ApplicationUserConfirmEmailInputModel model)
        {
            var response = await httpClient.PostAsJsonAsync("Identity/Authentication/confirm-email", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await errorHandlingService.HandleErrorResponse<bool>(response);
        }

        public async Task<ResponseModel<bool>> VerifyEmailCodeAsync(ApplicationUserConfirmEmailInputModel model)
        {
            var response = await httpClient.PostAsJsonAsync("Identity/Authentication/confirm-email-verify-code", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await errorHandlingService.HandleErrorResponse<bool>(response);
        }

        public async Task<ResponseModel<bool>> ForgotPasswordAsync(ApplicationUserForgotPasswordInputModel model)
        {
            var response = await httpClient.PostAsJsonAsync("Identity/Authentication/forgot-password", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await errorHandlingService.HandleErrorResponse<bool>(response);
        }

        public async Task<ResponseModel<bool>> ResetPasswordAsync(ApplicationUserForgotPasswordInputModel model)
        {
            var response = await httpClient.PostAsJsonAsync("Identity/Authentication/reset-password", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await errorHandlingService.HandleErrorResponse<bool>(response);
        }


    }
}
