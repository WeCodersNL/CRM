using CRM.Model.ApplicationModels;
using CRM.Model.InputModels;
using CRM.Model.ViewModels;

namespace CRM.WebBlazor.Service.Identity
{
    public interface IUserService
    {
        Task<ApplicationUserProfileViewModel> GetUserProfileAsync();
        Task<ResponseModel<bool>> UpdateUserProfileAsync(ApplicationUserProfileInputModel model);
        Task<ResponseModel<bool>> ChangePasswordAsync(ApplicationUserChangePasswordInputModel model);
    }
}
