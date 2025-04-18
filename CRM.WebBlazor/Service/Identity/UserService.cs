using CRM.Model.ApplicationModels;
using CRM.Model.InputModels;
using CRM.Model.ViewModels;
using Microsoft.JSInterop;
using System.Text.Json;

namespace CRM.WebBlazor.Service.Identity
{
    public class UserService(HttpClient http, IJSRuntime jsRuntime, IErrorHandlingService errorHandlingService) : IUserService
    {
        public async Task<ApplicationUserProfileViewModel> GetUserProfileAsync()
        {
            var userLocalStorage = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "user");
            if (!string.IsNullOrEmpty(userLocalStorage))
            {
                var user = JsonSerializer.Deserialize<ApplicationUserProfileViewModel>(userLocalStorage);
                if (user != null)
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);
                    return user;
                }
            }

            return new ApplicationUserProfileViewModel();
        }

        public async Task<ResponseModel<bool>> UpdateUserProfileAsync(ApplicationUserProfileInputModel model)
        {
            var response = await http.PostAsJsonAsync("Identity/User/update-user-profile", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await errorHandlingService.HandleErrorResponse<bool>(response);
        }

        public async Task<ResponseModel<bool>> ChangePasswordAsync(ApplicationUserChangePasswordInputModel model)
        {
            var response = await http.PostAsJsonAsync("Identity/User/change-password", model);

            if (response.IsSuccessStatusCode)
                return new ResponseModel<bool> { IsSuccess = true };

            return await errorHandlingService.HandleErrorResponse<bool>(response);
        }
    }
}
